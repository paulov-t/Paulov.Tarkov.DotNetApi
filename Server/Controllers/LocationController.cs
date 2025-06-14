using BSGHelperLibrary.ResponseModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Paulov.TarkovServices;
using Paulov.TarkovServices.Providers.Interfaces;
using Paulov.TarkovServices.Services;

namespace Paulov.Tarkov.WebServer.DOTNET.Controllers
{
    public class LocationController : ControllerBase
    {
        private SaveProvider _saveProvider;
        private IConfiguration configuration;

        public LocationController(ISaveProvider saveProvider, IConfiguration configuration)
        {
            _saveProvider = saveProvider as SaveProvider;
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
            if (!DatabaseProvider.TryLoadLocationBases(out var locationsJObjectByLocationMongoId))
            {
                Response.StatusCode = 500;
                return new BSGResult(null);
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
                            var str = JsonConvert.SerializeObject(z, Formatting.Indented, settings: new JsonSerializerSettings() { Converters = DatabaseProvider.CachedSerializer.Converters, NullValueHandling = NullValueHandling.Ignore });
                            bossSpawns.Add(JToken.Parse(str));
                        }
                        mapBase["Events"]["Halloween2024"]["MinInfectionPercentage"] = 99;
                        mapBase["Events"]["Halloween2024"]["InfectionPercentage"] = 100;
                    }
                }
            }

            if (!DatabaseProvider.TryLoadLocationPaths(out var paths))
            {
                Response.StatusCode = 500;
                return new BSGResult(null);
            }

            JObject result = new JObject();
            result.Add("locations", locationsJObjectByLocationMongoId);
            result.Add("paths", paths);

            return new BSGSuccessBodyResult(result);

        }
    }
}
