/*
 * File: ProsumerService.cs
 * Author:      Premaratne R.A.N.C (IT22050908)
 * Description: All business logic for prosumer accounts, pending activations, bookings and dashboard counts.
 *              Business rules live here (API only). Web and mobile apps just call the endpoints.
 * Created:     29/09/2026             
 */
using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Driver;
using SolarGrid.Api.Dtos;
using SolarGrid.Api.Models;

namespace SolarGrid.Api.Services;

public class ProsumerService
{
    private readonly IMongoCollection<User> _users;
    private readonly IMongoCollection<BookingRecord> _bookings;

    // Gets the two MongoDB collections this feature needs
    public ProsumerService(IMongoDatabase db)
    {
        _users = db.GetCollection<User>("Users");
        _bookings = db.GetCollection<BookingRecord>("EnergyReservations");
    }

    // Registers a new prosumer. RULE: every new prosumer starts as "Pending" until Backoffice activates
    public async Task<bool> RegisterAsync(RegisterRequest r)
    {
        if (await _users.Find(u => u.Nic == r.Nic).AnyAsync()) return false;   // NIC already used

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
        try { await _users.InsertOneAsync(user); }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey) { return false; }
        return true;
    }

    // Lists prosumers with an optional status filter and an optional search on name, NIC or email
    public async Task<List<ProsumerResponse>> GetProsumersAsync(string? search, string? status)
    {
        var fb = Builders<User>.Filter;
        var filter = fb.Eq(u => u.Role, "Prosumer");

        if (!string.IsNullOrWhiteSpace(status))
            filter &= fb.Eq(u => u.Status, status);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var rx = new BsonRegularExpression(Regex.Escape(search.Trim()), "i");
            filter &= fb.Or(fb.Regex(u => u.FullName, rx), fb.Regex(u => u.Email, rx), fb.Regex(u => u.Nic, rx));
        }

        var list = await _users.Find(filter).SortByDescending(u => u.CreatedAt).ToListAsync();
        return list.Select(ToResponse).ToList();
    }

    // Gets one prosumer by NIC (null if not found or not a prosumer)
    public async Task<ProsumerResponse?> GetByNicAsync(string nic)
    {
        var u = await _users.Find(x => x.Nic == nic && x.Role == "Prosumer").FirstOrDefaultAsync();
        return u == null ? null : ToResponse(u);
    }

    // Updates profile details (and optionally the password). Returns false if the prosumer does not exist
    public async Task<bool> UpdateProfileAsync(string nic, UpdateProfileRequest r)
    {
        var update = Builders<User>.Update
            .Set(u => u.FullName, r.FullName.Trim())
            .Set(u => u.Email, r.Email.Trim())
            .Set(u => u.Phone, r.Phone)
            .Set(u => u.Address, r.Address.Trim());

        if (!string.IsNullOrWhiteSpace(r.NewPassword))
            update = update.Set(u => u.PasswordHash, BCrypt.Net.BCrypt.HashPassword(r.NewPassword));

        var res = await _users.UpdateOneAsync(u => u.Nic == nic && u.Role == "Prosumer", update);
        return res.MatchedCount > 0;
    }

    // Activates a Pending or Deactivated prosumer. RULE: only called from a Backoffice-only endpoint,
    // so only Backoffice can approve new accounts and reactivate deactivated accounts
    public async Task<bool> ActivateAsync(string nic)
    {
        var update = Builders<User>.Update
            .Set(u => u.Status, "Active")
            .Set(u => u.DeactivationRequested, false);
        var res = await _users.UpdateOneAsync(u => u.Nic == nic && u.Role == "Prosumer" && u.Status != "Active", update);
        return res.MatchedCount > 0;
    }

    // Deactivates a prosumer account (Backoffice only)
    public async Task<bool> DeactivateAsync(string nic)
    {
        var update = Builders<User>.Update
            .Set(u => u.Status, "Deactivated")
            .Set(u => u.DeactivationRequested, false);
        var res = await _users.UpdateOneAsync(u => u.Nic == nic && u.Role == "Prosumer" && u.Status != "Deactivated", update);
        return res.MatchedCount > 0;
    }

    // A prosumer asks Backoffice to deactivate their account. Only Active accounts can ask
    public async Task<bool> RequestDeactivationAsync(string nic)
    {
        var update = Builders<User>.Update.Set(u => u.DeactivationRequested, true);
        var res = await _users.UpdateOneAsync(u => u.Nic == nic && u.Role == "Prosumer" && u.Status == "Active", update);
        return res.MatchedCount > 0;
    }

    // Gets a prosumer's bookings. type = "current" (Pending/Approved and in the future) or "history"
    // (Completed, Cancelled or in the past). Optional search on station name or status
    public async Task<List<BookingResponse>> GetMyBookingsAsync(string nic, string type, string? search)
    {
        var fb = Builders<BookingRecord>.Filter;
        var now = DateTime.UtcNow;
        var filter = fb.Eq(b => b.ProsumerNic, nic);
        bool history = string.Equals(type, "history", StringComparison.OrdinalIgnoreCase);

        if (history)
            filter &= fb.Or(fb.In(b => b.Status, new[] { "Completed", "Cancelled" }), fb.Lt(b => b.ReservationDate, now));
        else
            filter &= fb.In(b => b.Status, new[] { "Pending", "Approved" }) & fb.Gte(b => b.ReservationDate, now);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var rx = new BsonRegularExpression(Regex.Escape(search.Trim()), "i");
            filter &= fb.Or(fb.Regex(b => b.StationName, rx), fb.Regex(b => b.Status, rx));
        }

        var find = _bookings.Find(filter);
        var list = history
            ? await find.SortByDescending(b => b.ReservationDate).ToListAsync()
            : await find.SortBy(b => b.ReservationDate).ToListAsync();

        return list.Select(b => new BookingResponse
        {
            Id = b.Id,
            StationName = b.StationName,
            ReservationDate = b.ReservationDate,
            Status = b.Status,
            QrToken = b.QrToken
        }).ToList();
    }

    // Dashboard for one prosumer: their own pending bookings and approved future bookings
    public async Task<DashboardResponse> GetMyDashboardAsync(string nic)
    {
        var now = DateTime.UtcNow;
        return new DashboardResponse
        {
            PendingCount = await _bookings.CountDocumentsAsync(b => b.ProsumerNic == nic && b.Status == "Pending"),
            ApprovedFutureCount = await _bookings.CountDocumentsAsync(b => b.ProsumerNic == nic && b.Status == "Approved" && b.ReservationDate >= now)
        };
    }

    // Dashboard for Backoffice: prosumers waiting for activation and all approved future reservations
    public async Task<DashboardResponse> GetBackofficeDashboardAsync()
    {
        var now = DateTime.UtcNow;
        return new DashboardResponse
        {
            PendingCount = await _users.CountDocumentsAsync(u => u.Role == "Prosumer" && u.Status == "Pending"),
            ApprovedFutureCount = await _bookings.CountDocumentsAsync(b => b.Status == "Approved" && b.ReservationDate >= now)
        };
    }

    // Converts a User to a response object (never exposes the password hash)
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
