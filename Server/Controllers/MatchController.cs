using BSGHelperLibrary.ResponseModels;
using EFT;
using JsonType;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Paulov.Tarkov.WebServer.DOTNET.Middleware;
using Paulov.TarkovServices;
using Paulov.TarkovServices.Providers.Interfaces;
using Paulov.TarkovServices.Providers.SaveProviders;

namespace Paulov.Tarkov.WebServer.DOTNET.Controllers
{
    public class MatchController : ControllerBase
    {
        private JsonFileSaveProvider _saveProvider;
        public MatchController(ISaveProvider saveProvider)
        {
            _saveProvider = saveProvider as JsonFileSaveProvider;
        }


        [Route("client/match/group/invite/cancel-all")]
        [HttpPost]
        public async Task<IActionResult> CancelAllGroupInvites()
        {
            return new BSGSuccessBodyResult(new { });
        }


        /// <summary>
        /// Initiates a local match based on the provided location data in the request body.
        /// </summary>
        /// <remarks>This endpoint expects a POST request with a compressed request body containing a
        /// dictionary.  The dictionary must include a key named <c>"location"</c>, which specifies the location for the
        /// match. If the required key is missing or the request body cannot be processed, an error response is
        /// returned.</remarks>
        /// <returns>An <see cref="IActionResult"/> representing the result of the operation.  Returns a success response with an
        /// empty object if the operation completes successfully.  Returns an error response if the request body is
        /// invalid or required data is missing.</returns>
        [Route("client/match/local/start")]
        [HttpPost]
        public async Task<IActionResult> MatchLocalStart()
        {
            var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);
            if (requestBody == null)
                return new BSGErrorBodyResult(500, "");

            if (!requestBody.ContainsKey("location"))
                return new BSGErrorBodyResult(500, "expected location in request body");

            // Load all Location Bases
            DatabaseProvider.TryLoadLocationBases(out JObject locationsJO);

            // Match Location Base to requested location by Location Id
            // Todo: This needs refining
            var locationStringLower = requestBody["location"].ToString().ToLower();
            JToken location = null;
            foreach (var locationJO in locationsJO)
            {
                var l = locationJO.Value;
                if (locationStringLower.Contains(l["Id"].ToString()))
                {
                    location = l;
                }
            }

            // Generate the loot for the Location
            location["Loot"] = JToken.FromObject(Array.Empty<string>());

#if DEBUG
            // Paulov: I have left this here just as a reference
            _ = new LocalSettings();
#endif

            // Generate the result required by the Client
            var serverId = MongoID.Generate(false).ToString();
            JObject locationSettings = new JObject();
            locationSettings.Add("serverId", serverId);
            locationSettings.Add("locationLoot", location);
            //locationSettings.Add("profile", new JObject() { { "insuredItems", new JArray() } });
            locationSettings.Add("profile", new JObject() { });
            DatabaseProvider.TryLoadDatabaseFile("templates/locationServices.json", out JObject serverSettings);
            locationSettings.Add("serverSettings", serverSettings);
            //locationSettings.Add("transitionType", "None");
            locationSettings.Add("transition", new JObject() { });
            //locationSettings.Add("transition", new JObject()
            //{
            //    { "transitionType", (int)ELocationTransition.None },
            //    { "transitionRaidId", MongoID.Generate(false).ToString() },
            //    { "transitionCount", 0  },
            //    { "visitedLocations", new JArray() },
            //}
            //);

            //GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, true, true);

            //var r = new BSGSuccessBodyResult(JsonConvert.SerializeObject(locationLocalSettings, Formatting.Indented, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
            var r = new BSGSuccessBodyResult(JsonConvert.SerializeObject(locationSettings, Formatting.Indented, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
            return r;
        }

        [Route("client/match/local/end")]
        [HttpPost]
        public async Task<IActionResult> MatchLocalEnd()
        {
            return new BSGSuccessBodyResult(new { });
        }

        [Route("client/getMetricsConfig")]
        [HttpPost]
        public async Task<IActionResult> GetMetricsConfig()
        {
            DatabaseProvider.TryLoadDatabaseFile("match/metrics.json", out JObject dbFile);
            return new BSGSuccessBodyResult(dbFile);
        }

        [Route("client/match/group/exit_from_menu")]
        [HttpPost]
        public async Task<IActionResult> ExitFromMenu()
        {
            return new BSGSuccessBodyResult(new { });
        }
    }
}
