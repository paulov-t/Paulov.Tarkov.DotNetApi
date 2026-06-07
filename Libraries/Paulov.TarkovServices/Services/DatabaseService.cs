using EFT;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Paulov.TarkovServices.Providers.DatabaseProviders.CloudDatabaseProviders;
using Paulov.TarkovServices.Providers.DatabaseProviders.FileDatabaseProviders;
using Paulov.TarkovServices.Providers.DatabaseProviders.ZipDatabaseProviders;
using Paulov.TarkovServices.Providers.Interfaces;
using Paulov.TarkovServices.Services.Interfaces;
using System.Text.Json;


/**
 * 
 * Paulov: DatabaseService.cs
 * TODO: REWRITE THIS ENTIRE CLASS TO BE A SINGLETON SERVICE THAT CAN BE INJECTED INTO OTHER SERVICES.
 * REMOVE ALL STATIC METHODS AND PROPERTIES.
 * 
 */

namespace Paulov.TarkovServices.Services
{
    /// <summary>
    /// Provides methods and properties for accessing and managing database assets, including loading localized data,
    /// templates, and other resources from embedded or external sources.
    /// </summary>
    /// <remarks>The <see cref="DatabaseService"/> class is designed to facilitate interaction with
    /// database-related assets, such as JSON files and embedded resources. It includes functionality for loading and
    /// parsing data, converting paths, and handling localization files. Many methods in this class use streams or
    /// archives to access embedded resources. <para> This class is static and cannot be instantiated. It provides
    /// utility methods for working with database files and templates, including support for JSON deserialization and
    /// resource extraction. </para></remarks>
    public class DatabaseService : IDatabaseService
    {
        public readonly IConfiguration Configuration;

        public static DatabaseService Instance { get; private set; }

        public IDatabaseProvider DatabaseProvider;

        public DatabaseService(IConfiguration configuration, IDatabaseProvider databaseProvider)
        {
            this.DatabaseProvider = databaseProvider;
            this.Configuration = configuration;
            Instance = this;
        }

        private static IDatabaseProvider databaseProvider;

        public static IDatabaseProvider GetDatabaseProvider()
        {
            if (Instance != null && Instance.DatabaseProvider != null)
                return Instance.DatabaseProvider;

            // This is bad. Because we are using statics throughout DatabaseService there can be a loop to get the provider. We need to convert this service to a single instance
            if (databaseProvider != null)
                return databaseProvider;

            var configuration = Instance.Configuration;
            return GetDatabaseProviderByConfiguration(configuration);
        }

        public static IDatabaseProvider GetDatabaseProviderByConfiguration(IConfiguration configuration)
        {
            // This is bad. Because we are using statics throughout DatabaseService there can be a loop to get the provider. We need to convert this service to a single instance
            if (databaseProvider != null)
                return databaseProvider;

            // I don't know whether to put this here?
            if (configuration == null)
                return new MicrosoftCompressionZipDatabaseProvider();

            switch (configuration["DatabaseProvider"])
            {
                case "MongoDatabaseProvider":
                    databaseProvider = new MongoDatabaseProvider(configuration);
                    break;
                case "GitHubDatabaseProvider":
                    databaseProvider = new GitHubDatabaseProvider(configuration);
                    break;
                case "JsonFileCollectionDatabaseProvider":
                    databaseProvider = new JsonFileCollectionDatabaseProvider();
                    break;
                case "MicrosoftCompressionZipDatabaseProvider":
                default:
                    databaseProvider = new MicrosoftCompressionZipDatabaseProvider();
                    break;
            }

            return databaseProvider;
        }

        public static Newtonsoft.Json.JsonSerializer CachedSerializer;
        static JsonDocumentOptions CachedJsonDocumentOptions = new()
        {
            AllowTrailingCommas = false,
            CommentHandling = JsonCommentHandling.Skip,
            MaxDepth = 10
        };
        static JsonLoadSettings CachedJsonLoadSettings = new()
        {
            CommentHandling = CommentHandling.Ignore,
            DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Ignore,
        };

        static DatabaseService()
        {
            ITraceWriter traceWriter = new MemoryTraceWriter();
            CachedSerializer = new()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TraceWriter = traceWriter,
                ObjectCreationHandling = ObjectCreationHandling.Replace,
            };

            if (!CachedSerializer.Converters.Any())
            {
                var tarkovTypes = typeof(TarkovApplication).Assembly.DefinedTypes;
                var convertersType = tarkovTypes.FirstOrDefault(x => x.GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public).Any(p => p.Name == "Converters"));
                if (convertersType != null)
                {
                    var converters = (JsonConverter[])convertersType.GetField("Converters", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public).GetValue(null);
                    // the GClass1669`1 converter is calling an ECall error because its using Unity Loggers...
                    foreach (var converter in converters.Where(x => x.GetType().Name != "GClass1669`1"))
                        CachedSerializer.Converters.Add(converter);
                }
                CachedSerializer.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            }
        }




    }

}
