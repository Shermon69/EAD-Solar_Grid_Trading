/*
 * File:        DataSeeder.cs
 * Author:      [Your Name] ([IT Number])
 * Description: Adds sample data to an empty database when the API starts:
 *              users of every role, solar stations around Colombo, booking
 *              slots and a few reservations in different statuses.
 *              It only runs when the Users collection is empty.
 * Created:     28/09/2026
 */

using MongoDB.Driver;
using SolarGrid.Api.Models;

namespace SolarGrid.Api.Data
{
    /// <summary>
    /// Fills an empty database with sample data for development and the demo.
    /// </summary>
    public static class DataSeeder
    {
        // Password for every sample user
        public const string SamplePassword = "Password@123";

        // Sri Lanka time is UTC+5:30. Slot times are created in local time and stored in UTC
        private static readonly TimeSpan SriLankaOffset = new(5, 30, 0);

        /// <summary>
        /// Seeds all collections if the database is empty.
        /// </summary>
        public static async Task SeedAsync(MongoDbContext db)
        {
            if (await db.Users.CountDocumentsAsync(FilterDefinition<User>.Empty) > 0)
                return;

            var users = CreateUsers();
            await db.Users.InsertManyAsync(users);

            var stations = CreateStations();
            await db.Stations.InsertManyAsync(stations);

            var slots = CreateSlots(stations);
            await db.Slots.InsertManyAsync(slots);

            var reservations = CreateReservations(stations, slots);
            await db.Reservations.InsertManyAsync(reservations);

            // Booked slots have fewer available places (cancelled bookings don't count)
            foreach (var r in reservations.Where(r => r.Status != ReservationStatus.Cancelled))
            {
                await db.Slots.UpdateOneAsync(
                    s => s.Id == r.SlotId,
                    Builders<EnergySlot>.Update.Inc(s => s.AvailableSlots, -1));
            }
        }

        /// <summary>
        /// Creates sample users: 1 Backoffice, 2 Grid Operators and 4 Prosumers.
        /// </summary>
        private static List<User> CreateUsers()
        {
            var hash = BCrypt.Net.BCrypt.HashPassword(SamplePassword);

            return new List<User>
            {
                new() { Nic = "199012345678", FullName = "Nimal Perera", Email = "nimal@solargrid.lk", Phone = "0771234567",
                        Address = "Colombo 07", PasswordHash = hash, Role = Roles.Backoffice, Status = UserStatus.Active },
                new() { Nic = "199234567891", FullName = "Kasun Silva", Email = "kasun@solargrid.lk", Phone = "0772345678",
                        Address = "Malabe", PasswordHash = hash, Role = Roles.GridOperator, Status = UserStatus.Active },
                new() { Nic = "943456789V", FullName = "Dilini Fernando", Email = "dilini@solargrid.lk", Phone = "0773456789",
                        Address = "Nugegoda", PasswordHash = hash, Role = Roles.GridOperator, Status = UserStatus.Active },
                new() { Nic = "200045678912", FullName = "Tharindu Jayasinghe", Email = "tharindu@gmail.com", Phone = "0714567891",
                        Address = "Kaduwela", PasswordHash = hash, Role = Roles.Prosumer, Status = UserStatus.Active },
                new() { Nic = "985678912V", FullName = "Sachini Wijesekara", Email = "sachini@gmail.com", Phone = "0715678912",
                        Address = "Battaramulla", PasswordHash = hash, Role = Roles.Prosumer, Status = UserStatus.Active },
                new() { Nic = "200167891234", FullName = "Ravindu Bandara", Email = "ravindu@gmail.com", Phone = "0716789123",
                        Address = "Kottawa", PasswordHash = hash, Role = Roles.Prosumer, Status = UserStatus.Pending },
                new() { Nic = "199578912345", FullName = "Ishara Gunawardena", Email = "ishara@gmail.com", Phone = "0717891234",
                        Address = "Rajagiriya", PasswordHash = hash, Role = Roles.Prosumer, Status = UserStatus.Active,
                        DeactivationRequested = true }
            };
        }

        /// <summary>
        /// Creates five solar stations with real GPS locations around Colombo.
        /// </summary>
        private static List<SolarStation> CreateStations()
        {
            return new List<SolarStation>
            {
                NewStation("Malabe Solar Hub 01", "New Kandy Road, Malabe", 6.9147, 79.9729, 150, 20, "199234567891"),
                NewStation("Kaduwela Green Grid", "Avissawella Road, Kaduwela", 6.9336, 79.9847, 120, 16, "199234567891"),
                NewStation("Nugegoda Solar Point", "High Level Road, Nugegoda", 6.8649, 79.8997, 100, 12, "943456789V"),
                NewStation("Battaramulla Energy Hub", "Pannipitiya Road, Battaramulla", 6.9022, 79.9180, 180, 24, "943456789V"),
                NewStation("Colombo Fort Microgrid", "York Street, Colombo 01", 6.9344, 79.8428, 200, 30, "199234567891")
            };
        }

        /// <summary>
        /// Builds one station that is open 06:00-18:00 every day of the week.
        /// </summary>
        private static SolarStation NewStation(string name, string address, double lat, double lng,
            double capacityKw, int batterySlots, string operatorNic)
        {
            var days = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

            return new SolarStation
            {
                Name = name,
                Address = address,
                Latitude = lat,
                Longitude = lng,
                CapacityKw = capacityKw,
                BatterySlots = batterySlots,
                Schedule = days.Select(d => new OperatingHours { Day = d, OpenTime = "06:00", CloseTime = "18:00" }).ToList(),
                OperatorNics = new List<string> { operatorNic },
                IsActive = true
            };
        }

        /// <summary>
        /// Creates four 2-hour slots per day for every station, from 2 days ago to 7 days ahead.
        /// </summary>
        private static List<EnergySlot> CreateSlots(List<SolarStation> stations)
        {
            var slots = new List<EnergySlot>();
            var startHours = new[] { 8, 10, 13, 15 };
            var today = DateTimeOffset.UtcNow.ToOffset(SriLankaOffset).Date;

            foreach (var station in stations)
            {
                for (var day = -2; day <= 7; day++)
                {
                    foreach (var hour in startHours)
                    {
                        var start = new DateTimeOffset(today.AddDays(day).AddHours(hour), SriLankaOffset).UtcDateTime;
                        var places = station.BatterySlots / 4;

                        slots.Add(new EnergySlot
                        {
                            StationId = station.Id!,
                            StartTime = start,
                            EndTime = start.AddHours(2),
                            TotalSlots = places,
                            AvailableSlots = places,
                            IsAvailable = true
                        });
                    }
                }
            }

            return slots;
        }

        /// <summary>
        /// Creates sample reservations for the two active prosumers in every status.
        /// </summary>
        private static List<EnergyReservation> CreateReservations(List<SolarStation> stations, List<EnergySlot> slots)
        {
            var now = DateTime.UtcNow;

            // Finds the first slot of a station that starts after the given number of days from now
            EnergySlot SlotAfterDays(int stationIndex, double days) =>
                slots.Where(s => s.StationId == stations[stationIndex].Id && s.StartTime > now.AddDays(days))
                     .OrderBy(s => s.StartTime)
                     .First();

            EnergyReservation NewReservation(string nic, EnergySlot slot, string type, double kwh, string status)
            {
                var reservation = new EnergyReservation
                {
                    ProsumerNic = nic,
                    StationId = slot.StationId,
                    SlotId = slot.Id!,
                    ReservationTime = slot.StartTime,
                    Type = type,
                    EnergyKwh = kwh,
                    Status = status
                };

                if (status == ReservationStatus.Approved || status == ReservationStatus.Completed)
                    reservation.QrToken = Guid.NewGuid().ToString("N");

                if (status == ReservationStatus.Completed)
                {
                    reservation.CompletedBy = "199234567891";
                    reservation.CompletedAt = slot.StartTime.AddMinutes(30);
                }

                return reservation;
            }

            return new List<EnergyReservation>
            {
                // Tharindu
                NewReservation("200045678912", SlotAfterDays(0, 2), ReservationType.Charging, 12.5, ReservationStatus.Approved),
                NewReservation("200045678912", SlotAfterDays(1, 3), ReservationType.Dropoff, 8, ReservationStatus.Pending),
                NewReservation("200045678912", SlotAfterDays(0, -2), ReservationType.Charging, 10, ReservationStatus.Completed),
                NewReservation("200045678912", SlotAfterDays(2, 4), ReservationType.Charging, 6, ReservationStatus.Cancelled),

                // Sachini
                NewReservation("985678912V", SlotAfterDays(3, 1), ReservationType.Dropoff, 15, ReservationStatus.Approved),
                NewReservation("985678912V", SlotAfterDays(4, 5), ReservationType.Charging, 9.5, ReservationStatus.Pending),
                NewReservation("985678912V", SlotAfterDays(3, -1), ReservationType.Dropoff, 20, ReservationStatus.Completed)
            };
        }
    }
}
