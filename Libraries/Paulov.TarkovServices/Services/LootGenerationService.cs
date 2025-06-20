using EFT;
using Newtonsoft.Json.Linq;
using Paulov.TarkovServices.Helpers;
using Paulov.TarkovServices.Providers.Interfaces;
using Paulov.TarkovServices.Services.Interfaces;
using System.Diagnostics;
using System.Numerics;

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

            Debug.WriteLine($"Generating loot for location {location["Id"]}.");

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
                                , itemTemplates
                                ))
                                lootItems.Add(item);
                        }
                        break;
                }
            }

            return JArray.FromObject(lootItems);
        }

        private JArray GenerateContainerLoot(JToken containerData, JToken staticLootForContainer, float locationLootChanceModifierFromFile, string locationName, JObject templateItemList)
        {
            if (containerData == null || staticLootForContainer == null)
            {
                throw new ArgumentNullException("Container template or static loot for container cannot be null.");
            }

            JArray lootItems = new JArray();

            var containerLootItems = containerData["Items"] as JArray;
            var containerParentId = containerLootItems.First["_id"].ToString();
            var containerTemplateId = containerLootItems.First["_tpl"].ToString();

            var isWeaponBox = (containerTemplateId == "5909d5ef86f77467974efbd8");

            var containerTemplate = templateItemList[containerTemplateId];
            var containerTemplateProperties = containerTemplate["_props"];
            var containerTemplateGrid = ((JArray)containerTemplateProperties["Grids"])[0];
            bool[,] container2d = new bool[int.Parse(containerTemplateGrid["_props"]["cellsV"].ToString()), int.Parse(containerTemplateGrid["_props"]["cellsH"].ToString())];
            foreach (var item in GenerateContainerLootList(containerParentId, container2d, staticLootForContainer, templateItemList))
            {
                //containerLootItems.Add(containerData);
                containerLootItems.Add(item);
            }
            containerData["Items"] = containerLootItems;

            lootItems.Add(containerData);
            return lootItems;
        }

        private JArray GenerateContainerLootList(string containerParentId, bool[,] container2d, JToken staticLootForContainer, JObject templateItemList)
        {
            JArray lootItems = new JArray();

            var itemCountDistList = staticLootForContainer["itemcountDistribution"] as JArray;
            var itemCount = Math.Max(1, itemCountDistList[this.Randomizer.Next(0, itemCountDistList.Count - 1)].Count());
            var itemDistList = staticLootForContainer["itemDistribution"] as JArray;

            var orderedDistList = itemDistList
                .OrderByDescending(x => float.Parse(x["relativeProbability"].ToString()))
                .Select(x => x)
                .ToList();

            var maxProbability = int.Parse(orderedDistList[0]["relativeProbability"].ToString());
            var lowestProbability = int.Parse(orderedDistList[orderedDistList.Count - 1]["relativeProbability"].ToString());

            for (var i = 0; i < orderedDistList.Count && lootItems.Count < itemCount; i++)
            {
                var item = orderedDistList[i];
                var itemRelativeProbability = int.Parse(orderedDistList[0]["relativeProbability"].ToString());
                var calculatedProbability = Randomizer.Next((int)Math.Round(itemRelativeProbability * 0.9), (int)Math.Round(itemRelativeProbability * 1.5));
                // TODO: Calculate by rarity
                if (calculatedProbability < maxProbability)
                {
                    var templateItem = templateItemList[item["tpl"].ToString()];
                    var itemWidth = int.Parse(templateItem["_props"]["Width"].ToString());
                    var itemHeight = int.Parse(templateItem["_props"]["Height"].ToString());

                    if (new ContainerHelpers().PlaceItemInRandomSpotInContainer(container2d, itemWidth, itemHeight, out Vector2 position, out bool rotation))
                    {
                        JObject resultItem = new JObject();
                        resultItem["__DEBUG_Name"] = templateItem["_name"];
                        resultItem["_id"] = MongoID.Generate(false).ToString();
                        resultItem["_tpl"] = item["tpl"].ToString();
                        resultItem["parentId"] = containerParentId;
                        resultItem["location"] = new JObject();
                        resultItem["location"]["x"] = (int)Math.Round(position.X);
                        resultItem["location"]["y"] = (int)Math.Round(position.Y);
                        resultItem["location"]["r"] = (rotation ? 1 : 0);
                        resultItem["slotId"] = "main";

                        if (templateItem["_parent"].ToString() == "543be5dd4bdc2deb348b4569" || templateItem["_parent"].ToString() == "5485a8684bdc2da71d8b4567")
                        {
                            var stackMin = int.Parse(templateItem["_props"]["StackMinRandom"].ToString());
                            var stackMax = int.Parse(templateItem["_props"]["StackMaxRandom"].ToString());
                            var stackCount = Random.Shared.Next(stackMin, stackMax);
                            resultItem["upd"] = JToken.FromObject(new { StackObjectsCount = stackCount });
                        }

                        lootItems.Add(resultItem);
                    }
                }
            }


            return lootItems;
        }
    }
}
