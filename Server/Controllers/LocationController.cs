using BSGHelperLibrary.ResponseModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Paulov.TarkovServices.Providers.Interfaces;
using Paulov.TarkovServices.Providers.SaveProviders;
using Paulov.TarkovServices.Services;

namespace Paulov.Tarkov.WebServer.DOTNET.Controllers
{
    public class LocationController : ControllerBase
    {
        private JsonFileSaveProvider _saveProvider;
        private IConfiguration configuration;

        public LocationController(ISaveProvider saveProvider, IConfiguration configuration)
        {
            _saveProvider = saveProvider as JsonFileSaveProvider;
            this.configuration = configuration;
        }

        /// <summary>
        /// Provides an object of locations (base) and paths between each location
        /// </summary>
        /// <returns></returns>
        [Route("client/locations")]
        [HttpPost]
        public async Task<IActionResult> Locations()
        {
            if (!DatabaseService.TryLoadLocationBases(out var locationsJObjectByLocationMongoId))
            {
                Response.StatusCode = 500;
                return new BSGErrorBodyResult(500, "Failed to load locations from database.");
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

            //foreach (var mapBase in locationsJObjectByLocationMongoId.Values())
            //{
            //    var bossSpawns = ((JArray)mapBase["BossLocationSpawn"]);
            //    if (bossSpawns.Count == 0)
            //        continue;

            //    var cloneBossSpawn1 = bossSpawns[0];
            //    cloneBossSpawn1["BossName"] = "pmcUSEC";
            //    cloneBossSpawn1["BossChance"] = 999999;
            //    cloneBossSpawn1["Time"] = 1;
            //    bossSpawns.Add(cloneBossSpawn1);

            //}

            if (!DatabaseService.TryLoadLocationPaths(out var paths))
            {
                Response.StatusCode = 500;
                return new BSGErrorBodyResult(500, "Failed to load location paths from database.");
            }

            JObject result = new JObject();
            result.Add("locations", locationsJObjectByLocationMongoId);
            result.Add("paths", paths);

            return new BSGSuccessBodyResult(result);

        }
    }
}
