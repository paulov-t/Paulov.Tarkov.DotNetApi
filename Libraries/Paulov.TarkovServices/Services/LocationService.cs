using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Paulov.TarkovServices.Helpers;
using Paulov.TarkovServices.Services.Interfaces;

namespace Paulov.TarkovServices.Services
{
    public sealed class LocationService : ILocationService
    {
        private IDatabaseService databaseService;

        private readonly IConfiguration configuration;

        public LocationService(IConfiguration configuration, IGlobalsService globalsService, IInventoryService inventoryService, IDatabaseService databaseService, ILootGenerationService lootGenerationService)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            GlobalsService = globalsService ?? throw new ArgumentNullException(nameof(globalsService));
            InventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
            this.databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
            LootGenerationService = lootGenerationService ?? throw new ArgumentNullException(nameof(lootGenerationService));
        }

        public IGlobalsService GlobalsService { get; }
        public IInventoryService InventoryService { get; }
        public ILootGenerationService LootGenerationService { get; }

        public JObject LoadLocations()
        {
            if (!DatabaseHelpers.TryLoadLocationBases(out var locationsJObjectByLocationMongoId))
            {
                return null;
            }

            if (configuration["ZOMBIES_ONLY"] != null && bool.TryParse(configuration["ZOMBIES_ONLY"].ToString(), out var ZOMBIES_ONLY) && ZOMBIES_ONLY)
            {
                var allZombieSpawners = new BotGenerationService().GetZombieSpawners();
                if (allZombieSpawners != null && allZombieSpawners.Count > 0)
                {
                    foreach (var mapBase in locationsJObjectByLocationMongoId.Values())
                    {
                        if (!allZombieSpawners.ContainsKey(mapBase["Id"].ToString()))
                            continue;

                        var zombieSpawners = allZombieSpawners[mapBase["Id"].ToString()];
                        var bossSpawns = ((JArray)mapBase["BossLocationSpawn"]);
                        foreach (var z in zombieSpawners)
                        {
                            z.BossChance = 999999;
                            z.Time = 1;
                            var str = JsonConvert.SerializeObject(z, Formatting.Indented, settings: new JsonSerializerSettings() { Converters = DatabaseService.CachedSerializer.Converters, NullValueHandling = NullValueHandling.Ignore });
                            bossSpawns.Add(JToken.Parse(str));
                        }
                        mapBase["Events"]["Halloween2024"]["MinInfectionPercentage"] = 50;
                        mapBase["Events"]["Halloween2024"]["InfectionPercentage"] = 100;
                    }
                }
            }

            if (configuration["LABS_REQUIRES_KEYCARD"] != null && bool.TryParse(configuration["LABS_REQUIRES_KEYCARD"].ToString(), out var LABS_REQUIRES_KEYCARD))
            {
                foreach (var kvp in locationsJObjectByLocationMongoId)
                {
                    var mapBase = locationsJObjectByLocationMongoId[kvp.Key];
                    mapBase["AccessKeys"] = new JArray();
                    mapBase["AccessKeysPvE"] = new JArray();
                    locationsJObjectByLocationMongoId[kvp.Key] = mapBase;
                }
            }

            if (!DatabaseHelpers.TryLoadLocationPaths(out var paths))
            {
                return null;
            }

            JObject result = new JObject();
            result.Add("locations", locationsJObjectByLocationMongoId);
            result.Add("paths", paths);

            return result;
        }
    }
}
