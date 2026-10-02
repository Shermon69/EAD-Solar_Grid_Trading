/*
 * File:        StationService.cs
 * Author:      WMVSB Wahundeniya (IT22292872)
 * Description: Business logic for managing solar microgrid nodes (stations),
 *              including deactivation validation (R8) and location queries.
 * Created:     29/09/2026
 */

using MongoDB.Driver;
using SolarGrid.Api.Data;
using SolarGrid.Api.DTOs;
using SolarGrid.Api.Helpers;
using SolarGrid.Api.Models;

namespace SolarGrid.Api.Services
{
    public class StationService
    {
        private readonly IMongoCollection<SolarStation> _stations;
        private readonly IMongoCollection<EnergyReservation> _reservations;

        /// <summary>
        /// Initializes a new instance of StationService.
        /// </summary>
        public StationService(MongoDbContext context)
        {
            _stations = context.Stations;
            _reservations = context.Reservations;
        }

        /// <summary>
        /// Executes the GetAllAsync operation.
        /// </summary>
        public async Task<List<SolarStation>> GetAllAsync(bool activeOnly = false)
        {
            if (activeOnly)
            {
                return await _stations.Find(s => s.IsActive).ToListAsync();
            }
            return await _stations.Find(_ => true).ToListAsync();
        }

        /// <summary>
        /// Executes the GetByIdAsync operation.
        /// </summary>
        public async Task<SolarStation> GetByIdAsync(string id)
        {
            var station = await _stations.Find(s => s.Id == id).FirstOrDefaultAsync();
            if (station == null) throw BusinessRuleException.NotFound("Station not found.");
            return station;
        }

        /// <summary>
        /// Executes the CreateAsync operation.
        /// </summary>
        public async Task<SolarStation> CreateAsync(CreateStationRequest dto)
        {
            // Check GPS, capacity, battery slots and schedule (added by Shermon H)
            MicrogridValidator.ValidateStation(dto);

            var station = new SolarStation
            {
                Name = dto.Name,
                Address = dto.Address,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                CapacityKw = dto.CapacityKw,
                BatterySlots = dto.BatterySlots,
                Schedule = dto.Schedule,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _stations.InsertOneAsync(station);
            return station;
        }

        /// <summary>
        /// Executes the UpdateAsync operation.
        /// </summary>
        public async Task<SolarStation> UpdateAsync(string id, UpdateStationRequest dto)
        {
            var station = await GetByIdAsync(id);

            // Check GPS, capacity, battery slots and schedule (added by Shermon H)
            MicrogridValidator.ValidateStation(dto);

            station.Name = dto.Name;
            station.Address = dto.Address;
            station.Latitude = dto.Latitude;
            station.Longitude = dto.Longitude;
            station.CapacityKw = dto.CapacityKw;
            station.BatterySlots = dto.BatterySlots;
            station.Schedule = dto.Schedule;
            station.UpdatedAt = DateTime.UtcNow;

            await _stations.ReplaceOneAsync(s => s.Id == id, station);
            return station;
        }

        /// <summary>
        /// Rule R8: A node cannot be deactivated while it has Pending or Approved future reservations.
        /// </summary>
        public async Task DeactivateAsync(string id)
        {
            var station = await GetByIdAsync(id);

            var activeReservationsCount = await _reservations.CountDocumentsAsync(r =>
                r.StationId == id &&
                r.ReservationTime > DateTime.UtcNow &&
                (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Approved));

            if (activeReservationsCount > 0)
            {
                throw BusinessRuleException.Conflict("Cannot deactivate a station with active future reservations.");
            }

            station.IsActive = false;
            station.UpdatedAt = DateTime.UtcNow;
            await _stations.ReplaceOneAsync(s => s.Id == id, station);
        }

        /// <summary>
        /// Executes the ActivateAsync operation.
        /// </summary>
        public async Task ActivateAsync(string id)
        {
            var station = await GetByIdAsync(id);
            station.IsActive = true;
            station.UpdatedAt = DateTime.UtcNow;
            await _stations.ReplaceOneAsync(s => s.Id == id, station);
        }

        /// <summary>
        /// Basic Haversine distance-based search could be done in memory for small datasets,
        /// but since this is MongoDB we can just pull all and filter or use geospatial if 2dsphere index exists.
        /// To keep it simple, we will fetch all active and filter in-memory.
        /// </summary>
        public async Task<List<SolarStation>> GetNearbyAsync(double lat, double lng, double radiusKm)
        {
            var activeStations = await _stations.Find(s => s.IsActive).ToListAsync();

            var nearby = new List<SolarStation>();
            foreach (var s in activeStations)
            {
                var distance = CalculateDistance(lat, lng, s.Latitude, s.Longitude);
                if (distance <= radiusKm)
                {
                    nearby.Add(s);
                }
            }
            return nearby;
        }

        /// <summary>
        /// Executes the CalculateDistance operation.
        /// </summary>
        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var r = 6371; // Earth radius in km
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return r * c;
        }

        /// <summary>
        /// Executes the ToRadians operation.
        /// </summary>
        private double ToRadians(double angle)
        {
            return Math.PI * angle / 180.0;
        }
    }
}
