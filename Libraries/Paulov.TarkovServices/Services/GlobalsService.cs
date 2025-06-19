using Comfort.Common;
using Newtonsoft.Json.Linq;
using Paulov.TarkovServices.Providers.Interfaces;
using Paulov.TarkovServices.Services.Interfaces;

namespace Paulov.TarkovServices.Services
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class GlobalsService : IGlobalsService
    {
        IDatabaseProvider _databaseProvider;

        public GlobalsService(IDatabaseProvider databaseProvider)
        {
            this._databaseProvider = databaseProvider;
            LoadGlobalsIntoComfortSingleton();
        }

        public JObject LoadGlobals()
        {
            // TODO: Detect which Globals to load (EFT or Arena)
            if (DatabaseService.TryLoadDatabaseFile("globals.json", out JObject items))
            {
                if (!items.ContainsKey("LocationInfection"))
                    items.Add("LocationInfection", new JObject() { });

                if (!items.ContainsKey("time"))
                    items.Add("time", DateTime.Now.Ticks / 1000);

                return items;
            }

            return null;
        }

        /// <summary>
        /// Loads global configuration settings into the singleton instance.
        /// </summary>
        /// <remarks>This method initializes the singleton with global settings, ensuring that the instance
        /// is populated with the required configuration data. Call this method before accessing the singleton to ensure
        /// it is properly initialized.</remarks>
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
