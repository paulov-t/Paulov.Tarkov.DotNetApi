using Newtonsoft.Json.Linq;
using Paulov.TarkovServices.Providers.Interfaces;
using Paulov.TarkovServices.Services.Interfaces;
using System.Diagnostics;

namespace Paulov.TarkovServices.Services
{
    public class LootGenerationService : ILootGenerationService
    {
        private Random Randomizer = new Random();
        public LootGenerationService(IGlobalsService globalsService, IInventoryService inventoryService, IDatabaseService databaseService, IDatabaseProvider databaseProvider)
        {
            _globalsService = globalsService ?? throw new ArgumentNullException(nameof(globalsService));
            _globalsService.LoadGlobalsIntoComfortSingleton();
            _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
            _databaseProvider = databaseProvider ?? throw new ArgumentNullException(nameof(databaseProvider));
        }

        IGlobalsService _globalsService;
        IInventoryService _inventoryService;
        IDatabaseService _databaseService;
        IDatabaseProvider _databaseProvider;

        public JArray GenerateLootForLocation(JObject location)
        {
            if (location == null)
            {
                throw new ArgumentNullException(nameof(location), "Location cannot be null.");
            }

            Debug.WriteLine($"Generating loot for location {location["_id"]}.");

            JArray lootItems = new JArray();

            DatabaseService.TryLoadItemTemplates(out string templates);
            JObject itemTemplates = JObject.Parse(templates);

            var locationLootChanceModifierFromFile = float.Parse(location["GlobalLootChanceModifier"].ToString());
            var locationIdLower = location["Id"].ToString().ToLower();

            var looseLootDocument = DatabaseService.GetJsonDocument($"database/locations/{locationIdLower}/looseLoot.json");
            var staticAmmoDocument = DatabaseService.GetJsonDocument($"database/locations/{locationIdLower}/staticAmmo.json");
            var staticContainersDocument = DatabaseService.GetJsonDocument($"database/locations/{locationIdLower}/staticContainers.json");
            var staticLootDocument = DatabaseService.GetJsonDocument($"database/locations/{locationIdLower}/staticLoot.json");
            var staticsDocument = DatabaseService.GetJsonDocument($"database/locations/{locationIdLower}/statics.json");


            foreach (var r in staticContainersDocument.RootElement.EnumerateObject())
            {
                switch (r.Name)
                {
                    case "staticContainers":
                        var staticContainers = JArray.Parse(r.Value.GetRawText());
                        foreach (var container in staticContainers)
                        {
                            var probability = float.Parse(container["probability"].ToString());
                            if (Randomizer.NextFloat(0, 1) > probability)
                                continue;

                            var containerTemplate = container["template"] as JToken;
                            var templateItems = container["template"]["Items"] as JArray;

                            var containerId = templateItems[0]["_tpl"].ToString();
                            foreach (var item in GenerateContainerLoot(containerTemplate
                                , JObject.Parse(staticLootDocument.RootElement.GetRawText())[containerId]
                                , locationLootChanceModifierFromFile
                                , location["Name"].ToString()
                                ))
                                lootItems.Add(item);
                        }
                        break;
                }
            }
            //foreach (var container in staticContainersDocument["staticContainers"])
            //{
            //}

            return JArray.FromObject(lootItems);
        }

        private JArray GenerateContainerLoot(JToken containerTemplate, JToken staticLootForContainer, float locationLootChanceModifierFromFile, string locationName)
        {
            if (containerTemplate == null || staticLootForContainer == null)
            {
                throw new ArgumentNullException("Container template or static loot for container cannot be null.");
            }

            JArray lootItems = new JArray();
            lootItems.Add(containerTemplate);

            return lootItems;
        }
    }
}
