using Comfort.Common;
using FMT.FileTools;
using Newtonsoft.Json.Linq;
using Paulov.TarkovServices.Services.Interfaces;

namespace WebApiTests
{
    internal sealed class TestsGlobalsService : IGlobalsService
    {
        public static Stream GlobalsStream { get { return EmbeddedResourceHelper.GetEmbeddedResourceByName("globals.json"); } }

        public JObject LoadGlobals()
        {
            if (GlobalsStream == null)
            {
                return null;
            }
            using var reader = new StreamReader(GlobalsStream);
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
