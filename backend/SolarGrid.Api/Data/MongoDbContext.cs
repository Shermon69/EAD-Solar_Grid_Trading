/*
 * File:        MongoDbContext.cs
 * Author:      Shermon H (IT22177964)
 * Description: Connects to MongoDB and gives access to the four collections
 *              used by the system. Services get this class through
 *              dependency injection instead of creating their own connection.
 */

using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SolarGrid.Api.Models;

namespace SolarGrid.Api.Data
{
    /// <summary>
    /// Single place that holds the database connection and the collections.
    /// </summary>
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        /// <summary>
        /// Opens the connection using the settings from appsettings.json.
        /// </summary>
        public MongoDbContext(IOptions<MongoDbSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            _database = client.GetDatabase(settings.Value.DatabaseName);
        }

        // The four MongoDB collections used by the system
        public IMongoCollection<User> Users => _database.GetCollection<User>("Users");
        public IMongoCollection<SolarStation> Stations => _database.GetCollection<SolarStation>("SolarStationInfo");
        public IMongoCollection<EnergySlot> Slots => _database.GetCollection<EnergySlot>("EnergyBookingSlots");
        public IMongoCollection<EnergyReservation> Reservations => _database.GetCollection<EnergyReservation>("EnergyReservations");
    }
}
