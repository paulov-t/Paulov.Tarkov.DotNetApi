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
            //var languagesJsonText = File.ReadAllText(Path.Combine("locales", "languages.json"));
            //languages = JObject.Parse(languagesJsonText);

            //string basePath = localesPath;
            //var dirs = Directory.GetDirectories(localesPath);
            //foreach (var dir in dirs)
            //{
            //    var files = Directory.GetFiles(dir);
            //    foreach (var file in files)
            //    {
            //        string localename = dir.Replace(basePath + "\\", "");
            //        string localename_add = file.Replace(dir + "\\", "").Replace(".json", "");

            //        using (var sr = new StreamReader(file))
            //            locales.Add(localename + "_" + localename_add, sr.ReadToEnd());

            //        //localesDict.Add(localename + "_" + localename_add, JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(file)));
            //        localesDict.Add(localename + "_" + localename_add, JObject.Parse(File.ReadAllText(file)));

            //        result = true;
            //    }
            //    files = null;
            //}
            //dirs = null;

            return result;
        }

        public static bool TryLoadLocaleGlobalEn(
            out string globalEn)
        {
            bool result = false;
            globalEn = null;

            //var localesPath = Path.Combine(DatabaseAssetPath, "locales");
            //globalEn = File.ReadAllText(Path.Combine(localesPath, "global", "en.json"));

            return result;
        }

        public static bool TryLoadLanguages(
            out JObject languages)
        {
            languages = new();
            //var localesPath = Path.Combine(DatabaseAssetPath, "locales");
            //languages = JObject.Parse(File.ReadAllText((Path.Combine(localesPath, "languages.json"))));
            return true;
        }

        public static string ConvertPath(in string databaseFilePath)
        {
            //return databaseFilePath.Contains("\\") ? DatabaseAssetPath + "\\" + databaseFilePath : Path.Combine(DatabaseAssetPath, databaseFilePath);
            return Path.Combine("database", databaseFilePath).Replace("\\", "/");
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

        //public static bool TryLoadDatabaseFile<T>(
        //in string databaseFilePath,
        //out T model)
        //{
        //    bool result = false;
        //    model = default(T);

        //    var filePath = ConvertPath(databaseFilePath);

        //    var bytes = File.ReadAllBytes(filePath);
        //    JsonDocument jsonDocument = JsonDocument.Parse(bytes, CachedJsonDocumentOptions);
        //    result = bytes != null;

        //    var jobj = JObject.Parse(jsonDocument.RootElement.GetRawText(), CachedJsonLoadSettings);
        //    var temp = jobj.ToObject<Dictionary<string, object>>(CachedSerializer).SITToJson();
        //    model = temp.ParseJsonTo<T>();
        //    return result;
        //}

        public static bool TryLoadDatabaseFile(
        in string databaseFilePath,
        out Dictionary<string, object> templates)
        {
            bool result = false;

            var filePath = ConvertPath(databaseFilePath);
            if (!File.Exists(filePath))
            {
                templates = new Dictionary<string, object>();
                return false;
            }

            var stringTemplates = File.ReadAllText(filePath);
            result = stringTemplates != null;
            templates = JsonConvert.DeserializeObject<Dictionary<string, object>>(stringTemplates);

            return result;
        }

        public static bool TryLoadDatabaseFile(
        string databaseFilePath,
        out JObject dbFile)
        {
            bool result = false;
            var filePath = ConvertPath(databaseFilePath);

            var jsonDocument = GetJsonDocument(databaseFilePath);
            dbFile = jsonDocument.Deserialize<JObject>();

            if (!File.Exists(filePath))
            {
                dbFile = null;
                return false;
            }

            dbFile = JObject.Parse(File.ReadAllText(filePath));
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

            var filePath = Path.Combine(DatabaseAssetPath, databaseFilePath);

            dbFile = JArray.Parse(File.ReadAllText(filePath));
            result = dbFile != null;
            return result;
        }

        public static bool TryLoadDatabaseFile(
        in string databaseFilePath,
        out string stringTemplates)
        {
            bool result = false;

            var filePath = Path.Combine(DatabaseAssetPath, databaseFilePath);
            stringTemplates = File.ReadAllText(filePath);
            result = stringTemplates != null;
            return result;
        }


        public static bool TryLoadTemplateFile(
         in string templateFile,
         out Dictionary<string, object> templates)
        {
            bool result = false;

            var filePath = Path.Combine(DatabaseAssetPath, "templates", templateFile);

            var stringTemplates = File.ReadAllText(filePath);
            result = stringTemplates != null;
            templates = JsonConvert.DeserializeObject<Dictionary<string, object>>(stringTemplates);

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
            var itemsPath = Path.Combine(DatabaseAssetPath, "templates", "items.json");

            var stringTemplates = File.ReadAllText(itemsPath);

            templatesRaw = stringTemplates;
            return templatesRaw != null;
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

        public static bool TryLoadLocations(
         out JObject locations)
        {
            JObject locationsRaw = new();
            foreach (var dir in Directory.GetDirectories(Path.Combine(DatabaseAssetPath, "locations")).Select(x => new DirectoryInfo(x)))
            {
                locationsRaw.Add(dir.Name, new JObject());
                foreach (var f in Directory.GetFiles(dir.FullName).Select(x => new FileInfo(x)))
                {
                    try
                    {
                        var ob = locationsRaw[dir.Name].ToObject<JObject>();
                        var readText = JsonDocument.Parse(File.ReadAllBytes(f.FullName)).RootElement.GetRawText();
                        var n = f.Name.Replace(".json", "");
                        if (readText.StartsWith("["))
                        {
                            ob.Add(n, JArray.Parse(readText));
                        }
                        else
                        {
                            ob.Add(n, JObject.Parse(readText));
                        }
                        locationsRaw[dir.Name] = ob;
                        readText = null;
                    }
                    catch
                    {

                    }
                }
            }

            locations = locationsRaw;
            return locations.Count > 0;
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
