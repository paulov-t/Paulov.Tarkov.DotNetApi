﻿using EFT;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Paulov.TarkovServices.Models;
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

        /// <summary>
        /// Attempts to load locale data, including global and menu-specific locales, for all available languages.
        /// </summary>
        /// <remarks>This method attempts to load locale data from a database archive. If the database
        /// archive is unavailable or the required files are missing, the method returns <see langword="false"/>. The
        /// locale data is organized into global and menu-specific locales for each language.</remarks>
        /// <param name="locales">When this method returns, contains a <see cref="JObject"/> representing the loaded locale data, if the
        /// operation succeeds; otherwise, an empty <see cref="JObject"/>.</param>
        /// <param name="localesDict">When this method returns, contains a <see cref="JObject"/> mapping locale keys (e.g., "global_en",
        /// "menu_en") to their corresponding locale data, if the operation succeeds; otherwise, an empty <see
        /// cref="JObject"/>.</param>
        /// <param name="languages">When this method returns, contains a <see cref="JObject"/> representing the available languages, if the
        /// operation succeeds; otherwise, an empty <see cref="JObject"/>.</param>
        /// <returns><see langword="true"/> if the locale data is successfully loaded; otherwise, <see langword="false"/>.</returns>
        public static bool TryLoadLocales(
            out JObject locales
            , out JObject localesDict
            , out JObject languages)
        {
            bool result = false;


            locales = new();
            localesDict = new();
            languages = new();

            var db = new SharpCompressZipDatabaseProvider();
            if (db == null)
                return false;

            var languagesJsonPath = Path.Combine("database", "locales", "languages.json");
            TryLoadDatabaseFile(languagesJsonPath, out languages);

            foreach (var language in languages)
            {
                var key = language.Key;
                try
                {
                    var menuPath = Path.Combine("database", "locales", "menu", $"{key}.json");
                    localesDict.Add("menu_" + language.Key, JObject.Parse(GetJsonDocument(menuPath).RootElement.GetRawText()));
                    var globalPath = Path.Combine("database", "locales", "global", $"{key}.json");
                    localesDict.Add("global_" + language.Key, JObject.Parse(GetJsonDocument(globalPath).RootElement.GetRawText()));
                }
                catch
                {

                }
            }

            return result;
        }

        public static bool TryLoadLocaleGlobalEn(
            out string globalEn)
        {
            globalEn = null;
            var localesPath = Path.Combine("database", "locales", "global", "en.json");
            globalEn = GetJsonDocument(localesPath).RootElement.GetRawText();
            return true;
        }

        public static bool TryLoadLanguages(
            out JObject languages)
        {
            languages = new();
            var db = GetDatabaseProvider();
            if (db == null)
                return false;

            var languagesJsonPath = Path.Combine("database", "locales", "languages.json");
            return TryLoadDatabaseFile(languagesJsonPath, out languages);
        }

        public static string ConvertPath(in string databaseFilePath)
        {
            return Path.Combine("database", databaseFilePath).Replace('\\', '/').Replace("database/database", "database");
        }

        public static JsonDocument GetJsonDocument(string databaseFilePath)
        {
            var filePath = ConvertPath(databaseFilePath);

            var ms = new MemoryStream();

            var databaseProvider = GetDatabaseProvider();

            // If the databaseprovider uses entries then attempt to find it there
            var entry = databaseProvider.Entries.FirstOrDefault(x => x.FullName == filePath);
            if (entry != null)
            {
                var stream = entry.Open();
                stream.CopyTo(ms);
                stream.Dispose();
                stream = null;
            }
            // If the databaseprovider doesn't support entries, then attempt to get directly
            else
            {
                databaseProvider.GetEntryStream(filePath).CopyTo(ms);
            }

            if (ms.Length == 0)
            {
                throw new FileNotFoundException($"Database file not found: {filePath}");
            }

            var jsonDocument = JsonDocument.Parse(new ReadOnlyMemory<byte>(ms.ToArray()));
            ms.Dispose();
            ms = null;

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
            return jsonDocument;
        }

        public static bool TryLoadDatabaseFile(
        in string databaseFilePath,
        out JObject templates)
        {
            bool result = false;

            using JsonDocument jsonDocument = GetJsonDocument(databaseFilePath);
            result = TryLoadDatabaseFile(databaseFilePath, out templates);

            return result;
        }

        public static bool TryLoadDatabaseFile(
        string databaseFilePath,
        out JObject dbFile)
        {
            bool result = false;
            var filePath = ConvertPath(databaseFilePath);

            using JsonDocument jsonDocument = GetJsonDocument(databaseFilePath);
            var rawText = jsonDocument.RootElement.GetRawText();
            dbFile = JObject.Parse(rawText, CachedJsonLoadSettings);

            result = dbFile != null;
            return result;
        }

        public static IEnumerable<KeyValuePair<string, JToken>> LoadDatabaseFileAsEnumerable(string databaseFilePath)
        {
            string filePath = ConvertPath(databaseFilePath);

            EntryModel entry = GetDatabaseProvider().Entries.FirstOrDefault(x => x.FullName == filePath);
            if (entry == null) yield break;

            using Stream dbFileStream = entry.Open();
            using StreamReader sr = new(dbFileStream);
            using JsonTextReader reader = new(sr);

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    string key = (string)reader.Value;
                    reader.Read(); //Move the reader to the value
                    JToken obj = JToken.ReadFrom(reader);
                    yield return new KeyValuePair<string, JToken>(key, obj);
                }
                else if (reader.TokenType == JsonToken.EndObject)
                {
                    yield break;
                }
            }
        }

        public static bool TryLoadDatabaseFile(
        in string databaseFilePath,
        out JsonDocument jsonDocument)
        {
            bool result = false;
            jsonDocument = GetJsonDocument(databaseFilePath);
            result = jsonDocument != null;
            return result;
        }

        public static bool TryLoadDatabaseFile(
        in string databaseFilePath,
        out JArray dbFile)
        {
            bool result = false;

            dbFile = JArray.Parse(GetJsonDocument(databaseFilePath).RootElement.GetRawText());
            result = dbFile != null;
            return result;
        }

        public static bool TryLoadDatabaseFile(
        in string databaseFilePath,
        out string stringTemplates)
        {
            bool result = false;

            stringTemplates = GetJsonDocument(databaseFilePath).RootElement.GetRawText();
            result = stringTemplates != null;
            return result;
        }


        //public static bool TryLoadTemplateFile(
        // in string templateFile,
        // out Dictionary<string, object> templates)
        //{
        //    bool result = false;

        //    var filePath = Path.Combine("templates", templateFile);
        //    var document = GetJsonDocument(filePath);
        //    result = document != null;
        //    templates = JsonConvert.DeserializeObject<Dictionary<string, object>>(document.RootElement.GetRawText());

        //    return result;
        //}

        public static bool TryLoadTemplateFile(
         in string templateFile,
         out JObject templates)
        {
            bool result = false;

            var filePath = Path.Combine("templates", templateFile);
            using JsonDocument document = GetJsonDocument(filePath);
            result = document != null;
            templates = JObject.Parse(document.RootElement.GetRawText());

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="templates"></param>
        /// <param name="count">Used for Swagger tests - count on page</param>
        /// <param name="page">Used for Swagger tests - page</param>
        /// <returns></returns>
        public static bool TryLoadItemTemplates(
            out string templatesRaw,
            in int? count = null,
            in int? page = null
            )
        {
            var templates = new JObject();
            TryLoadTemplateFile("items.json", out templates);
            templatesRaw = JsonConvert.SerializeObject(templates);
            return true;
        }

        public static bool TryLoadCustomization(
          out JObject customization)
        {
            return TryLoadTemplateFile("customization.json", out customization);
        }

        public static bool TryLoadGlobals(
         out JObject globals)
        {
            return TryLoadDatabaseFile("globals.json", out globals);
            //return TryLoadDatabaseFile("globalsArena.json", out globals);
        }

        //public static bool TryLoadGlobalsArena(
        // out Dictionary<string, object> globals)
        //{
        //    var result = TryLoadDatabaseFile("globals.json", out globals);

        //    if (TryLoadDatabaseFile("globalsArena.json", out Dictionary<string, object> globalsArena))
        //        globals.Add("GlobalsArena", globalsArena);


        //    if (!globals.ContainsKey("GameModes"))
        //    {
        //        globals.Add("GameModes",
        //            new Dictionary<string, object>()
        //            {
        //            { new MongoID(true), new ArenaGameModeSettings() {
        //            } }
        //            }
        //            );
        //    }
        //    return result;
        //}

        /// <summary>
        /// Attempts to load location base data from the database archive.
        /// </summary>
        /// <remarks>This method scans the database archive for entries under the "database/locations/"
        /// directory and attempts  to load location base data from files named "base.json". Only entries with valid
        /// names are processed. If a file does not contain valid location data, it is skipped.</remarks>
        /// <param name="locations">When the method returns, contains a dictionary where the keys are location names and the values are  the
        /// corresponding location data objects. If no location data is found, the dictionary will be empty.</param>
        /// <returns><see langword="true"/> if one or more location bases were successfully loaded; otherwise, <see
        /// langword="false"/>.</returns>
        public static bool TryLoadLocationBases(
        out JObject locations)
        {
            JObject locationsRaw = new();

            var locationEntries = GetDatabaseProvider().Entries.Where(x => x.FullName.StartsWith("database/locations/")).ToArray();
            foreach (var entry in locationEntries.Where(x => !string.IsNullOrEmpty(x.Name)))
            {
                if (entry.Name == "base.json")
                {
                    TryLoadDatabaseFile(entry.FullName, out JObject dbFile);
                    if (dbFile.ContainsKey("locations"))
                        continue;

                    string name = entry.FullName
                        .Replace("database/locations/", "")
                        .Replace("/base.json", "");

                    try
                    {
                        locationsRaw.Add(dbFile["_Id"].ToString(), dbFile);
                    }
                    catch
                    {

                    }
                }
            }

            locations = locationsRaw;
            return locations.Count > 0;
        }

        public static bool TryLoadLocationPaths(out JToken paths)
        {
            paths = null;
            if (!TryLoadDatabaseFile("locations/base.json", out var @base))
                return false;

            paths = @base["paths"];
            return true;
        }

        public static bool TryLoadWeather(
         out Dictionary<string, Dictionary<string, object>> weather)
        {
            weather = new();
            return true;
        }

        public static bool TryLoadTraders(
        out JObject traders)
        {
            traders = new JObject();

            var entries = GetDatabaseProvider().Entries.Where(x => x.FullName.StartsWith("database/traders/"));
            foreach (var entry in entries)
            {
                var entryName = entry.FullName.Replace("database/traders/", "").Replace("/base.json", "");
                if (entry.Name == "base.json")
                {
                    traders.Add(entryName, JToken.Parse(GetJsonDocument(entry.FullName).RootElement.GetRawText()));
                }
            }

            return true;
        }

        /// <summary>
        /// This uses a LOT of memory. Needs fixing.
        /// </summary>
        /// <param name="templateId"></param>
        /// <returns></returns>
        public static JObject GetTemplateItemById(string templateId)
        {
            TryLoadTemplateFile("items.json", out var templates);
            var template = GetTemplateItemById(templates, templateId);

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, true, true);

            return template;
        }

        public static JObject GetTemplateItemById(JObject templates, string templateId)
        {
            var template = templates[templateId] as JObject;
            return template;
        }

        public static int GetTemplateItemPrice(string templateId)
        {
            TryLoadDatabaseFile("templates/prices.json", out JObject templatesPricesData);
            if (templatesPricesData.ContainsKey(templateId))
            {
                var resultPriceString = templatesPricesData[templateId].ToString();
                if (int.TryParse(resultPriceString, out int resultPrice))
                {
                    return resultPrice;
                }
            }

            return 1;
        }

        public static List<JObject> GetTemplateItemsAsArray()
        {
            TryLoadTemplateFile("items.json", out var templates);
            return GetTemplateItemsAsArray(templates);
        }

        public static List<JObject> GetTemplateItemsAsArray(JObject templates)
        {
            List<JObject> templatesItems = new List<JObject>();
            foreach (var template in templates)
            {
                templatesItems.Add((JObject)template.Value);
            }
            return templatesItems;
        }


    }

}
