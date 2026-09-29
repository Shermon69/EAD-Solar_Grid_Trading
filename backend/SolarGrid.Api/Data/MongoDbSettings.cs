/*
 * File:        MongoDbSettings.cs
 * Author:      Shermon H (IT22177964)
 * Description: Holds the MongoDB connection settings read from appsettings.json.
 * Created:     28/09/2026
 */

namespace SolarGrid.Api.Data
{
    /// <summary>
    /// Maps to the "MongoDbSettings" section of appsettings.json.
    /// </summary>
    public class MongoDbSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
    }
}
