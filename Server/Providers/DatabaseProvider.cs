using FMT.FileTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Paulov.Tarkov.WebServer.DOTNET.BSG;
using SIT.Arena;
using System.IO.Compression;
using System.Text.Json;

namespace Paulov.Tarkov.WebServer.DOTNET.Providers
{
    public class DatabaseProvider
    {
        public static string DatabaseAssetPath { get { return Path.Combine(AppContext.BaseDirectory, "assets", "database"); } }
        //public static string DatabaseAssetPath { get { return Path.Combine(AppContext.BaseDirectory, "assets", "database.zip"); } }

        public static Stream DatabaseAssetStream { get { return EmbeddedResourceHelper.GetEmbeddedResourceByName("database.zip"); } }
        public static ZipArchive DatabaseAssetZipArchive { get { return new ZipArchive(DatabaseAssetStream); } }

        //public static Dictionary<string, object> Database { get; } = new Dictionary<string, object>();
        //JsonSerializerOptions CachedOptions = new JsonSerializerOptions { WriteIndented = false };
        static Newtonsoft.Json.JsonSerializer CachedSerializer;
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
            CachedSerializer = new()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            };
            CachedSerializer.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());

        }

        private static T StreamFileToType<T>(string path)
        {
            if (typeof(T) == typeof(JObject))
            {
            }

            using (var readerLanguagesJson = new StreamReader(path))
            {
                using (var readerLanguagesJsonTR = new JsonTextReader(readerLanguagesJson))
                {
                    return CachedSerializer.Deserialize<T>(readerLanguagesJsonTR);
                }
            }
            //return System.Text.Json.JsonDocument.Parse(File.ReadAllText(path)).Deserialize<T>();
        }


        public static bool TryLoadLocales(
            out JObject locales
            , out JObject localesDict
            , out JObject languages)
        {
            bool result = false;


            locales = new();
            localesDict = new();
            languages = new();

            var db = DatabaseAssetZipArchive;
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
            var db = DatabaseAssetZipArchive;
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

            using var zipArchive = new System.IO.Compression.ZipArchive(DatabaseAssetStream);
            var zipEntry = zipArchive.Entries.First(x => x.FullName == filePath);

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


        public static bool TryLoadTemplateFile(
         in string templateFile,
         out Dictionary<string, object> templates)
        {
            bool result = false;

            var filePath = Path.Combine("templates", templateFile);
            var document = GetJsonDocument(filePath);
            result = document != null;
            templates = JsonConvert.DeserializeObject<Dictionary<string, object>>(document.RootElement.GetRawText());

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

            //out Dictionary<string, object> templates,
            out string templatesRaw,
            in int? count = null,
            in int? page = null
            )
        {
            var templates = new Dictionary<string, object>();
            TryLoadTemplateFile("items.json", out templates);
            templatesRaw = JsonConvert.SerializeObject(templates);
            return true;
        }

        public static bool TryLoadCustomization(
          out Dictionary<string, object> customization)
        {
            return TryLoadTemplateFile("customization.json", out customization);
        }

        public static bool TryLoadGlobals(
         out JObject globals)
        {
            return TryLoadDatabaseFile("globals.json", out globals);
            //return TryLoadDatabaseFile("globalsArena.json", out globals);
        }

        public static bool TryLoadGlobalsArena(
         out Dictionary<string, object> globals)
        {
            var result = TryLoadDatabaseFile("globals.json", out globals);

            if (TryLoadDatabaseFile("globalsArena.json", out Dictionary<string, object> globalsArena))
                globals.Add("GlobalsArena", globalsArena);


            if (!globals.ContainsKey("GameModes"))
            {
                globals.Add("GameModes",
                    new Dictionary<string, object>()
                    {
                    { new MongoID(true), new ArenaGameModeSettings() {
                    } }
                    }
                    );
            }
            return result;
        }

        public static bool TryLoadLocationBases(
        out Dictionary<string, object> locations)
        {
            Dictionary<string, object> locationsRaw = new();
            foreach (var dir in Directory.GetDirectories(Path.Combine(DatabaseAssetPath, "locations")).Select(x => new DirectoryInfo(x)))
            {
                locationsRaw.Add(dir.Name, new LocationSettingsClass.Location());
                foreach (var f in Directory.GetFiles(dir.FullName)
                    .Where(x => x.EndsWith("base.json"))
                    .Select(x => new FileInfo(x)))
                {
                    try
                    {
                        string relativePath = f.FullName.Replace(DatabaseProvider.DatabaseAssetPath, "");
                        DatabaseProvider.TryLoadDatabaseFile(relativePath, out Dictionary<string, object> model);
                        var ob = locationsRaw[dir.Name] = model;
                    }
                    catch
                    {

                    }
                }
            }

            locations = locationsRaw;
            return locations.Count > 0;
        }

        public static bool TryLoadWeather(
         out Dictionary<string, Dictionary<string, object>> weather)
        {
            weather = new();
            return true;
        }
    }

}
