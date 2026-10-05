/*
 * File:        ProsumerService.cs
 * Author:      Premaratne R.A.N.C (IT22050908)
 * Description: All business logic for prosumer accounts, pending activations,
 *              bookings and dashboard counts. Business rules live here (API only).
 *              Web and mobile apps just call the endpoints.
 */

using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Driver;
using SolarGrid.Api.Data;
using SolarGrid.Api.Dtos;
using SolarGrid.Api.Models;

namespace SolarGrid.Api.Services;

/// <summary>
/// Service that handles all prosumer-related business logic. All data
/// access for the Users and EnergyReservations collections happens here.
/// </summary>
public class ProsumerService
{
    private readonly IMongoCollection<User> _users;
    private readonly IMongoCollection<BookingRecord> _bookings;
    private readonly IMongoCollection<SolarStation> _stations;

    // Injects the MongoDB database and resolves the collections needed.
    public ProsumerService(MongoDbContext dbContext)
    {
        _users = dbContext.Users;

        // Read EnergyReservations through the BookingRecord projection.
        _bookings = dbContext.Reservations.Database
            .GetCollection<BookingRecord>("EnergyReservations");

        _stations = dbContext.Stations;
    }

    /// <summary>
    /// Registers a new prosumer. Rule: every new prosumer starts as
    /// "Pending" until Backoffice activates the account.
    /// </summary>
    public async Task<bool> RegisterAsync(RegisterRequest r)
    {
        // Store the NIC in upper case (e.g. 991234567V) because login upper-cases
        // the NIC it searches for. Updating the request also lets the caller
        // look up the new prosumer with the same value.
        r.Nic = r.Nic.Trim().ToUpper();

        // Reject if the NIC is already used.
        if (await _users.Find(u => u.Nic == r.Nic).AnyAsync())
            return false;

        var user = new User
        {
            Nic = r.Nic,
            FullName = r.FullName.Trim(),
            Email = r.Email.Trim(),
            Phone = r.Phone,
            Address = r.Address.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(r.Password),
            Role = "Prosumer",
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            await _users.InsertOneAsync(user);
        }
        catch (MongoWriteException ex)
            when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Lists prosumers with an optional status filter and an optional
    /// search on name, NIC or email.
    /// </summary>
    public async Task<List<ProsumerResponse>> GetProsumersAsync(
        string? search,
        string? status)
    {
        var fb = Builders<User>.Filter;

        var filter = fb.Eq(
            u => u.Role,
            "Prosumer"
        );

        if (!string.IsNullOrWhiteSpace(status))
        {
            filter &= fb.Eq(
                u => u.Status,
                status
            );
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var rx = new BsonRegularExpression(
                Regex.Escape(search.Trim()),
                "i"
            );

            filter &= fb.Or(
                fb.Regex(u => u.FullName, rx),
                fb.Regex(u => u.Email, rx),
                fb.Regex(u => u.Nic, rx)
            );
        }

        var list = await _users
            .Find(filter)
            .SortByDescending(u => u.CreatedAt)
            .ToListAsync();

        return list
            .Select(ToResponse)
            .ToList();
    }

    /// <summary>
    /// Gets one prosumer by NIC. Returns null if not found or not a prosumer.
    /// </summary>
    public async Task<ProsumerResponse?> GetByNicAsync(string nic)
    {
        var u = await _users
            .Find(x =>
                x.Nic == nic &&
                x.Role == "Prosumer")
            .FirstOrDefaultAsync();

        return u == null
            ? null
            : ToResponse(u);
    }

    /// <summary>
    /// Updates profile details and optionally the password. Returns false
    /// if the prosumer does not exist.
    /// </summary>
    public async Task<bool> UpdateProfileAsync(
        string nic,
        UpdateProfileRequest r)
    {
        var update = Builders<User>.Update
            .Set(u => u.FullName, r.FullName.Trim())
            .Set(u => u.Email, r.Email.Trim())
            .Set(u => u.Phone, r.Phone)
            .Set(u => u.Address, r.Address.Trim());

        if (!string.IsNullOrWhiteSpace(r.NewPassword))
        {
            update = update.Set(
                u => u.PasswordHash,
                BCrypt.Net.BCrypt.HashPassword(r.NewPassword)
            );
        }

        var res = await _users.UpdateOneAsync(
            u =>
                u.Nic == nic &&
                u.Role == "Prosumer",
            update
        );

        return res.MatchedCount > 0;
    }

    /// <summary>
    /// Activates a Pending or Deactivated prosumer. Rule: only Backoffice
    /// can approve new accounts and reactivate deactivated ones.
    /// </summary>
    public async Task<bool> ActivateAsync(string nic)
    {
        var update = Builders<User>.Update
            .Set(u => u.Status, "Active")
            .Set(u => u.DeactivationRequested, false);

        var res = await _users.UpdateOneAsync(
            u =>
                u.Nic == nic &&
                u.Role == "Prosumer" &&
                u.Status != "Active",
            update
        );

        return res.MatchedCount > 0;
    }

    /// <summary>
    /// Deactivates a prosumer account. Rule: Backoffice only.
    /// </summary>
    public async Task<bool> DeactivateAsync(string nic)
    {
        var update = Builders<User>.Update
            .Set(u => u.Status, "Deactivated")
            .Set(u => u.DeactivationRequested, false);

        var res = await _users.UpdateOneAsync(
            u =>
                u.Nic == nic &&
                u.Role == "Prosumer" &&
                u.Status != "Deactivated",
            update
        );

        return res.MatchedCount > 0;
    }

    /// <summary>
    /// Prosumer requests account deactivation. Rule: only Active accounts
    /// can request. Backoffice must approve before the account is disabled.
    /// </summary>
    public async Task<bool> RequestDeactivationAsync(string nic)
    {
        var update = Builders<User>.Update
            .Set(
                u => u.DeactivationRequested,
                true
            );

        var res = await _users.UpdateOneAsync(
            u =>
                u.Nic == nic &&
                u.Role == "Prosumer" &&
                u.Status == "Active",
            update
        );

        return res.MatchedCount > 0;
    }

    /// <summary>
    /// Gets a prosumer's bookings. type = "current" (Pending/Approved and
    /// in the future) or "history" (Completed, Cancelled or in the past).
    /// Optional search on station name or status. Looks up the station name
    /// from SolarStationInfo because the reservation only stores StationId.
    /// </summary>
    public async Task<List<BookingResponse>> GetMyBookingsAsync(
        string nic,
        string type,
        string? search)
    {
        var fb = Builders<BookingRecord>.Filter;

        var now = DateTime.UtcNow;

        var filter = fb.Eq(
            b => b.ProsumerNic,
            nic
        );

        bool history =
            string.Equals(
                type,
                "history",
                StringComparison.OrdinalIgnoreCase
            );

        if (history)
        {
            filter &= fb.Or(
                fb.In(
                    b => b.Status,
                    new[]
                    {
                        "Completed",
                        "Cancelled"
                    }
                ),
                fb.Lt(
                    b => b.ReservationDate,
                    now
                )
            );
        }
        else
        {
            filter &=
                fb.In(
                    b => b.Status,
                    new[]
                    {
                        "Pending",
                        "Approved"
                    }
                )
                &
                fb.Gte(
                    b => b.ReservationDate,
                    now
                );
        }

        var find = _bookings.Find(filter);

        var list = history
            ? await find
                .SortByDescending(b => b.ReservationDate)
                .ToListAsync()
            : await find
                .SortBy(b => b.ReservationDate)
                .ToListAsync();

        // Look up station names in one query to avoid N+1 queries.
        var stationIds = list
            .Select(b => b.StationId)
            .Distinct()
            .ToList();

        var stationCollection =
            _bookings.Database
                .GetCollection<SolarStation>(
                    "SolarStationInfo"
                );

        var stations = await stationCollection
            .Find(
                Builders<SolarStation>.Filter.In(
                    s => s.Id,
                    stationIds
                )
            )
            .ToListAsync();

        var stationLookup =
            stations.ToDictionary(
                s => s.Id,
                s => s.Name
            );

        // Fill in the display fields and apply the search filter.
        var results = list
            .Select(b => new BookingResponse
            {
                Id = b.Id,

                StationName =
                    stationLookup.GetValueOrDefault(
                        b.StationId,
                        "Unknown Station"
                    ),

                ReservationDate =
                    b.ReservationDate,

                // Include the reservation type so that
                // mobile and web clients can display it.
                Type = b.Type,

                // Include the requested energy amount so that
                // mobile and web clients can display it.
                EnergyKwh = b.EnergyKwh,

                Status = b.Status,

                QrToken = b.QrToken
            })
            .ToList();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();

            results = results
                .Where(r =>
                    r.StationName.Contains(
                        term,
                        StringComparison.OrdinalIgnoreCase
                    )
                    ||
                    r.Status.Contains(
                        term,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                .ToList();
        }

        return results;
    }

    /// <summary>
    /// Dashboard for one prosumer: their own pending bookings and approved
    /// future bookings, read live from the API.
    /// </summary>
    public async Task<DashboardResponse> GetMyDashboardAsync(
        string nic)
    {
        var now = DateTime.UtcNow;

        return new DashboardResponse
        {
            PendingCount =
                await _bookings.CountDocumentsAsync(
                    b =>
                        b.ProsumerNic == nic &&
                        b.Status == "Pending"
                ),

            ApprovedFutureCount =
                await _bookings.CountDocumentsAsync(
                    b =>
                        b.ProsumerNic == nic &&
                        b.Status == "Approved" &&
                        b.ReservationDate >= now
                )
        };
    }

    /// <summary>
    /// Dashboard for Backoffice: prosumers waiting for activation and all
    /// approved future reservations across the system.
    /// </summary>
    public async Task<DashboardResponse> GetBackofficeDashboardAsync()
    {
        var now = DateTime.UtcNow;

        return new DashboardResponse
        {
            PendingCount =
                await _users.CountDocumentsAsync(
                    u =>
                        u.Role == "Prosumer" &&
                        u.Status == "Pending"
                ),

            ApprovedFutureCount =
                await _bookings.CountDocumentsAsync(
                    b =>
                        b.Status == "Approved" &&
                        b.ReservationDate >= now
                )
        };
    }

    /// <summary>
    /// Converts a User document to a ProsumerResponse. Never exposes the
    /// password hash.
    /// </summary>
    private static ProsumerResponse ToResponse(User u) => new()
    {
        Nic = u.Nic,
        FullName = u.FullName,
        Email = u.Email,
        Phone = u.Phone,
        Address = u.Address,
        Status = u.Status,
        DeactivationRequested = u.DeactivationRequested,
        CreatedAt = u.CreatedAt
    };
}