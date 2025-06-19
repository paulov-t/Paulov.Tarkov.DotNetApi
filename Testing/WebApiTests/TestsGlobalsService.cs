using Comfort.Common;
using Newtonsoft.Json.Linq;
using Paulov.TarkovServices.Providers.DatabaseProviders.FileDatabaseProviders;
using Paulov.TarkovServices.Services.Interfaces;

namespace WebApiTests
{
    internal sealed class TestsGlobalsService : IGlobalsService
    {
        public JObject LoadGlobals()
        {
            var databaseProvider = new JsonFileCollectionDatabaseProvider();
            databaseProvider.Connect(AppContext.BaseDirectory);

            using var reader = new StreamReader(databaseProvider.GetEntryStream("globals.json"));
            var json = reader.ReadToEnd();
            return JObject.Parse(json);
        }

        public JObject LoadGlobalsIntoComfortSingleton()
        {
            var items = LoadGlobals();
            if (!Singleton<BackendConfigSettingsClass>.Instantiated)
            {
                Singleton<BackendConfigSettingsClass>.Create(items["config"].ToObject<BackendConfigSettingsClass>());
                _ = Singleton<BackendConfigSettingsClass>.Instance;
            }
            return items;
        }
    }
}
