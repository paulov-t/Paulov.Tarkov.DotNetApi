using EFT;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Paulov.TarkovServices.Providers.Interfaces;
using Paulov.TarkovServices.Providers.ZipDatabaseProviders;


//using System.IO.Compression;
using System.Text.Json;

namespace Paulov.TarkovServices
{
    /// <summary>
    /// Provides methods and properties for accessing and managing database assets, including loading localized data,
    /// templates, and other resources from embedded or external sources.
    /// </summary>
    /// <remarks>The <see cref="DatabaseProvider"/> class is designed to facilitate interaction with
    /// database-related assets, such as JSON files and embedded resources. It includes functionality for loading and
    /// parsing data, converting paths, and handling localization files. Many methods in this class use streams or
    /// archives to access embedded resources. <para> This class is static and cannot be instantiated. It provides
    /// utility methods for working with database files and templates, including support for JSON deserialization and
    /// resource extraction. </para></remarks>
    public class DatabaseProvider
    {
        /// <summary>
        /// 
        /// </summary>
        //public static Stream DatabaseAssetStream { get { return EmbeddedResourceHelper.GetEmbeddedResourceByName("database.zip"); } }

        /// <summary>
        /// 
        /// </summary>
        //public static ZipArchive DatabaseAssetZipArchive
        //public static IReader DatabaseAssetZipArchive
        //{
        //    get
        //    {
        //        var reader = ReaderFactory.Open(DatabaseAssetStream);
        //        return reader;

        //        //return ZipReader.Open(DatabaseAssetStream);
        //        //return new ZipArchive(DatabaseAssetStream, ZipArchiveMode.Read, false, System.Text.Encoding.ASCII);
        //    }
        //}

        public static IDatabaseProvider GetDatabaseProvider()
        {
            return new MicrosoftCompressionZipDatabaseProvider();
        }

        public static Newtonsoft.Json.JsonSerializer CachedSerializer;
        static JsonDocumentOptions CachedJsonDocumentOptions = new()
        {
            AllowTrailingCommas = false,
            CommentHandling = JsonCommentHandling.Skip,
            MaxDepth = 20
        };
        static JsonLoadSettings CachedJsonLoadSettings = new()
        {
            CommentHandling = CommentHandling.Ignore,
            DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Ignore,
        };

        static DatabaseProvider()
        {
            ITraceWriter traceWriter = new MemoryTraceWriter();
            CachedSerializer = new()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TraceWriter = traceWriter
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

            var languagesEntry = db.Entries.First(x => x.Name == "languages.json");
            TryLoadDatabaseFile(languagesEntry.FullName, out languages);

            foreach (var language in languages)
            {
                var localeEntries = db.Entries.Where(x => x.FullName.EndsWith(".json") && x.FullName.StartsWith("database/locales"));
                localeEntries = localeEntries.Where(x => x.FullName.EndsWith(language.Key + ".json"));
                foreach (var localeEntry in localeEntries)
                {
                    if (localeEntry.FullName.Contains("global"))
                        localesDict.Add("global_" + language.Key, JObject.Parse(GetJsonDocument(localeEntry.FullName).RootElement.GetRawText()));
                    else if (localeEntry.FullName.Contains("menu"))
                        localesDict.Add("menu_" + language.Key, JObject.Parse(GetJsonDocument(localeEntry.FullName).RootElement.GetRawText()));
                }
            }

            return result;
        }

        public static bool TryLoadLocaleGlobalEn(
            out string globalEn)
        {
            globalEn = null;
            var localesPath = Path.Combine("database", "locales", "global", "en.json");
            //globalEn = File.ReadAllText(Path.Combine(localesPath, "global", "en.json"));
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

            var languagesEntry = db.Entries.First(x => x.Name == "languages.json");
            return TryLoadDatabaseFile(languagesEntry.FullName, out languages);
        }

        public static string ConvertPath(in string databaseFilePath)
        {
            return Path.Combine("database", databaseFilePath).Replace("\\", "/").Replace("database/database", "database");
        }

        public static JsonDocument GetJsonDocument(string databaseFilePath)
        {
            var filePath = ConvertPath(databaseFilePath);

            var zipEntry = GetDatabaseProvider().Entries.First(x => x.FullName == filePath);

            using var ms = new MemoryStream();
            using var stream = zipEntry.Open();
            stream.CopyTo(ms);

            var jsonDocument = System.Text.Json.JsonDocument.Parse(new ReadOnlyMemory<byte>(ms.ToArray()));
            return jsonDocument;
        }

        public static bool TryLoadDatabaseFile(
        in string databaseFilePath,
        out Dictionary<string, object> templates)
        {
            bool result = false;

            var jsonDocument = GetJsonDocument(databaseFilePath);
            TryLoadDatabaseFile(databaseFilePath, out JObject dbFile);
            templates = dbFile.ToObject<Dictionary<string, object>>();

            return result;
        }

        public static bool TryLoadDatabaseFile(
        string databaseFilePath,
        out JObject dbFile)
        {
            bool result = false;
            var filePath = ConvertPath(databaseFilePath);

            var jsonDocument = GetJsonDocument(databaseFilePath);
            var rawText = jsonDocument.RootElement.GetRawText();
            dbFile = JObject.Parse(rawText, CachedJsonLoadSettings);

            result = dbFile != null;
            return result;
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
            var document = GetJsonDocument(filePath);
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
            DatabaseProvider.TryLoadDatabaseFile("templates/prices.json", out JObject templatesPricesData);
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
