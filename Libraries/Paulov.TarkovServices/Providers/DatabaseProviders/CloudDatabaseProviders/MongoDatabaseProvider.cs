using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using Paulov.TarkovServices.Models;
using Paulov.TarkovServices.Providers.Interfaces;
using Paulov.TarkovServices.Services;
using System.Data;
using System.Diagnostics;
using System.Text;

namespace Paulov.TarkovServices.Providers.DatabaseProviders.CloudDatabaseProviders
{
    /// <summary>
    /// Provides functionality for connecting to and interacting with a MongoDB database.
    /// </summary>
    /// <remarks>This class supports connecting to a MongoDB instance using a connection string provided
    /// either through an <see cref="IConfiguration"/> object or directly as a string. It allows retrieving database
    /// entries, executing commands, and managing connections.
    /// 
    /// It does however rely on the Mongo Database being structured in a way that it can be queried as a database. i.e. it has a collection of JSON files that can be queried as a database.
    /// Example: database/globals.json, database/templates/items.json, etc.
    /// 
    /// </remarks>
    public sealed class MongoDatabaseProvider : IDatabaseProvider
    {
        public List<EntryModel> Entries { get; set; } = new List<EntryModel>();

        private string _connectionString;

        public MongoDatabaseProvider(IConfiguration configuration)
        {
            Connect(configuration["MongoDBConnectionString"]);
        }

        public MongoDatabaseProvider(string connectionString)
        {
            _connectionString = connectionString;
            Connect(null);
        }

        private MongoClient GetClient()
        {
            var settings = MongoClientSettings.FromConnectionString(_connectionString);
            // Set the ServerApi field of the settings object to set the version of the Stable API on the client
            settings.ServerApi = new ServerApi(ServerApiVersion.V1);
            // Create a new client and connect to the server
            var client = new MongoClient(settings);
            return client;
        }

        public void Connect(string connectionString)
        {
            if (!string.IsNullOrEmpty(connectionString))
                _connectionString = connectionString;

            if (string.IsNullOrEmpty(_connectionString))
                throw new ArgumentNullException(nameof(_connectionString));

            var client = GetClient();
            try
            {
                var result = client.GetDatabase("admin").RunCommand<BsonDocument>(new BsonDocument("ping", 1));

                var globalsCollection = client.GetDatabase("Tarkov")
                    .GetCollection<BsonDocument>("globals.json")
                    .FindSync(FilterDefinition<BsonDocument>.Empty)
                    .FirstOrDefault()
                    .ToJson(DatabaseService.CachedSerializer.Converters.ToArray());

                client.GetDatabase("Tarkov").ListCollectionNames().ToList().ForEach(name =>
                {
                    Debug.WriteLine($"Collection: {name}");
                    if (name.Contains(".json"))
                    {
                        var fullName = name;
                        var nameSplitIndex = fullName.LastIndexOf('/');
                        if (nameSplitIndex >= 0)
                            name = fullName.Substring(nameSplitIndex + 1);

                        Entries.Add(new EntryModel(name, fullName, this));
                    }
                });

                var successMessage = "Pinged your deployment. You successfully connected to MongoDB!";
                Debug.WriteLine(successMessage);
                Console.WriteLine(successMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void Disconnect()
        {
            throw new NotImplementedException();
        }

        public void ExecuteCommand(string query)
        {
            throw new NotImplementedException();
        }

        public DataTable GetData(string query)
        {
            throw new NotImplementedException();
        }

        public Stream GetEntryStream(string entryName)
        {
            var client = GetClient();

            var collection = client.GetDatabase("Tarkov")
                   .GetCollection<BsonDocument>(entryName)
                   .Find<BsonDocument>(FilterDefinition<BsonDocument>.Empty)
                   .First();

            var json = collection.ToString();// ToJson(DatabaseService.CachedSerializer.Converters.ToArray());

            return new MemoryStream(Encoding.UTF8.GetBytes(json));
        }
    }
}
