using Comfort.Common;
using Newtonsoft.Json.Linq;

namespace Paulov.TarkovServices.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class GlobalsService
    {
        /// <summary>
        /// Gets the singleton instance of the <see cref="GlobalsService"/> class.
        /// </summary>
        public static GlobalsService Instance { get; private set; }

        static GlobalsService()
        {
            Instance = new GlobalsService();
            Instance.LoadGlobalsIntoComfortSingleton();
        }

        public JObject LoadGlobals()
        {
            // TODO: Detect which Globals to load
            if (DatabaseProvider.TryLoadDatabaseFile("globals.json", out JObject items))
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
