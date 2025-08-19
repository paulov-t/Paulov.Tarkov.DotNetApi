using BSGHelperLibrary.ResponseModels;
using JsonType;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Paulov.Tarkov.WebServer.DOTNET.Middleware;
using Paulov.TarkovServices;
using Paulov.TarkovServices.Helpers;
using Paulov.TarkovServices.Providers.Interfaces;
using Paulov.TarkovServices.Services;
using Paulov.TarkovServices.Services.Interfaces;
using System.Diagnostics;

namespace Paulov.Tarkov.WebServer.DOTNET.Controllers
{
    [ApiController]
    public class GameController : ControllerBase
    {
        private readonly ISaveProvider _saveProvider;
        private readonly IConfiguration configuration;
        private readonly IGlobalsService _globalsService;
        private readonly IInventoryService _inventoryService;
        private readonly IMatchingService _matchingService;
        private readonly IActionCommandService _actionCommandService;

        public GameController
            (
            ISaveProvider saveProvider
            , IConfiguration configuration
            , IGlobalsService globalsService
            , IInventoryService inventoryService
            , IMatchingService matchingService
            , IActionCommandService actionCommandService
            )
        {
            this._saveProvider = saveProvider;
            this.configuration = configuration;
            this._globalsService = globalsService;
            this._inventoryService = inventoryService;
            this._matchingService = matchingService;
            this._actionCommandService = actionCommandService;
        }

        private string SessionId
        {
            get
            {
                return HttpSessionHelpers.GetSessionId(Request, HttpContext);
            }
        }

        private int AccountId
        {
            get
            {
                var aid = HttpContext.Session.GetInt32("AccountId");
                return aid.Value;
            }
        }

        [Route("client/game/start", Name = "GameStart")]
        [HttpPost]
        public async Task<IActionResult> Start()
        {
            var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);

            var timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0);

            //await HttpBodyConverters.CompressDictionaryIntoResponseBodyBSG(
            //    new Dictionary<string, object>() { { "utc_time", (int)timeSpan.TotalSeconds } }
            //    , Request, Response);

            return new BSGSuccessBodyResult(new JObject() { { "utc_time", (int)timeSpan.TotalSeconds } });

        }

        [Route("client/game/version/validate")]
        [HttpPost]
        public async void VersionValidate()
        {
            await HttpBodyConverters.CompressNullIntoResponseBodyBSG(Request, Response);
        }

        [Route("client/game/config")]
        [Route("client/game/configuration")]
        [HttpPost]
        public async Task<IActionResult> GameConfig()
        {
            var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);

            string protocol = Request?.Protocol?.ToString();
            string ip = Request?.Host.ToString();
            string backendUrl = $"https://{ip}/";

            var sessionId = SessionId;
#if !DEBUG
            if (string.IsNullOrEmpty(sessionId))
            {
                Response.StatusCode = 412; // Precondition
                return StatusCode(500);
            }
#else
            if (string.IsNullOrEmpty(sessionId))
                sessionId = _saveProvider?.GetProfiles().First().Key;
#endif

            var profile = _saveProvider?.LoadProfile(sessionId);

            var config = new Dictionary<string, object>()
            {
                { "queued", false }
                , { "banTime", -1 }
                , { "hash", "" }
                , { "lang", "en" }
                , { "aid", profile?.AccountId }
                , { "token", profile?.AccountId }
                , { "taxonomy", 6 }
                , { "activeProfileId", $"{SessionId}" }
                , { "purchasedGames", new Dictionary<string, bool>(){ { "eft", true }, { "arena", true } } }
                , { "utc_time", DateTime.UtcNow.Ticks / 1000 }
                , { "totalInGame", 1 }
                , { "isGameSynced", true }
                , { "backend",
                    new { Lobby = backendUrl, Trading = backendUrl, Messaging = backendUrl, Main = backendUrl, Ragfair = backendUrl }
                }
            };

            //await HttpBodyConverters.CompressIntoResponseBodyBSG(config, Request, Response);
            return new BSGSuccessBodyResult(config.ToJson());
        }

        [Route("client/items")]
        [HttpPost]
        public async Task<IActionResult> TemplateItems(int? count, int? page)
        {

            if (DatabaseHelpers.TryLoadItemTemplates(out var items, count, page))
            {
                //var dict = items.ParseJsonTo<GClass1372>();
                //var dict = JsonConvert.DeserializeObject<GClass1372>(items, new JsonSerializerSettings() { Converters = DatabaseProvider.CachedSerializer.Converters, ReferenceLoopHandling = ReferenceLoopHandling.Ignore,   });
                //if (!Singleton<ItemFactoryClass>.Instantiated)
                //{
                //    Singleton<ItemFactoryClass>.Create(new ItemFactoryClass(dict));
                //}

                return new BSGSuccessBodyResult(items);
            }

            return new BSGErrorBodyResult(500, "");

        }

        [Route("client/globals")]
        [HttpPost]
        public IActionResult Globals()
        {
            var globals = _globalsService.LoadGlobalsIntoComfortSingleton();
            if (configuration["ZOMBIES_ONLY"] != null && configuration["ZOMBIES_ONLY"].ToString() == "true")
            {
                var infection = globals["config"]["SeasonActivity"]["InfectionHalloween"];
                infection["DisplayUIEnabled"] = true;
                infection["Enabled"] = true;

                var locationInfection = globals["LocationInfection"];
                var infectionKeys = ((JObject)locationInfection).Properties().Select(p => p.Name).ToArray();

                foreach (var key in infectionKeys)
                {
                    globals["LocationInfection"][key] = 100;
                }
            }

            return new BSGSuccessBodyResult(globals);
        }

        [Route("client/settings")]
        [HttpPost]
        public IActionResult Settings(int? retry, bool? debug)
        {
            DatabaseHelpers.TryLoadDatabaseFile("settings.json", out JObject items);

            var rawText = items.ToJson();
            return new BSGSuccessBodyResult(rawText);
        }

        [Route("client/game/keepalive")]
        [HttpPost]
        public IActionResult KeepAlive()
        {
            JObject obj = new();
            obj.TryAdd("msg", "OK");
            obj.TryAdd("utc_time", DateTime.UtcNow.Ticks / 1000);

            return new BSGSuccessBodyResult(obj);

        }
        [Route("client/account/customization")]
        [HttpPost]
        public IActionResult AccountCustomization(int? retry, bool? debug)
        {
            DatabaseHelpers.TryLoadDatabaseFile("templates/character.json", out string items);

            return new BSGSuccessBodyResult(items);
        }


        [Route("client/game/profile/select")]
        [HttpPost]
        public async Task<IActionResult> ProfileSelect()
        {
            var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);

            Dictionary<string, dynamic> response = new();
            response.Add("status", "ok");
            try
            {
                var packet = new NotifierProvider().CreateNotifierPacket(Request, Response, SessionId);
                response.Add("notifier", packet);
                response.Add("notifierServer", $"{packet["notifierServer"]}");
            }
            catch (Exception)
            {
                response.Add("notifier", new JObject());
                response.Add("notifierServer", new JObject());
            }
            requestBody = null;
            return new BSGSuccessBodyResult(JsonConvert.SerializeObject(response));
        }


        [Route("client/weather")]
        [HttpPost]
        public async Task<IActionResult> Weather(int? retry)
        {
            var requestBody = await HttpBodyConverters.DecompressRequestBody(Request);

            var weather = new WeatherClass()
            {
                Time = DateTime.Now.Ticks,
                Cloudness = 0.01f
            };
            var locationWeatherTime = new LocationWeatherTime(weather, 1, DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToShortTimeString());
            locationWeatherTime.SeasonsSettings = new();
            Debug.WriteLine(locationWeatherTime.ToJson());

            return new BSGSuccessBodyResult(locationWeatherTime.ToJson());
        }


        [Route("client/handbook/templates")]
        [HttpPost]
        public async Task<IActionResult> HandbookTemplates(int? retry, bool? debug)
        {
            DatabaseHelpers.TryLoadTemplateFile("handbook.json", out var templates);

            return new BSGSuccessBodyResult(templates);




        }

        /// <summary>
        /// Arena
        /// </summary>
        [Route("client/handbook/builds/my/list")]
        [HttpPost]
        public async void UserPresets()
        {
            Dictionary<string, object> nullResult = new();
            nullResult.Add("equipmentBuilds", new JArray());
            nullResult.Add("weaponBuilds", new JArray());
            await HttpBodyConverters.CompressIntoResponseBodyBSG(JsonConvert.SerializeObject(nullResult), Request, Response);

        }

        /// <summary>
        /// Creates a WebSocket channel for Notifications between Server and Client
        /// </summary>
        /// <returns></returns>
        [Route("client/notifier/channel/create")]
        [HttpPost]
        public IActionResult NotifierChannelCreate()
        {
            return new BSGSuccessBodyResult(new NotifierProvider().CreateNotifierPacket(Request, Response, SessionId));

        }




        /// <summary>
        /// Arena
        /// </summary>
        /// <param name="retry"></param>
        /// <param name="debug"></param>
        [Route("client/trading/customization/storage")]
        [HttpPost]
        public async void CustomizationStorage(int? retry, bool? debug)
        {
            Dictionary<string, object> packetResult = new();
            //packetResult.Add("_id", $"{SessionId}");
            //packetResult.Add("suites", saveProvider.GetAccountProfileMode(SessionId).Characters.PMC.Suits);
            await HttpBodyConverters.CompressIntoResponseBodyBSG(JsonConvert.SerializeObject(packetResult), Request, Response);
        }



        [Route("client/server/list")]
        [HttpPost]
        public async Task<IActionResult> ServerList(int? retry, bool? debug)
        {
            JArray result = new JArray();

            _matchingService.Servers.ForEach(server =>
            {
                result.Add(JObject.FromObject(server));
            });

            return new BSGSuccessBodyResult(result.ToJson());
        }




        [Route("client/repeatalbeQuests/activityPeriods")]
        [HttpPost]
        public async Task<IActionResult> RepeatableQuestList(int? retry, bool? debug)
        {
            return new BSGSuccessBodyResult(new JArray());
        }

        [Route("/client/items/prices/{traderId}")]
        [HttpPost]
        public async Task<IActionResult> ItemPricesForTraderId(int? retry, bool? debug)
        {
            var tradingProvider = new TradingProvider(_inventoryService);
            JObject handbookPrices = JObject.Parse(tradingProvider.GetStaticPrices().ToJson());
            Dictionary<string, object> packet = new();
            packet.Add("supplyNextTime", (int)Math.Floor(((DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds / 1000) + 60000));
            packet.Add("prices", handbookPrices);
            packet.Add("currencyCourses",
                new Dictionary<string, object>() {
                    { "5449016a4bdc2d6f028b456f", handbookPrices["5449016a4bdc2d6f028b456f"] },
                    {  "569668774bdc2da2298b4568", handbookPrices["569668774bdc2da2298b4568"] },
                    {  "5696686a4bdc2da3298b456a", handbookPrices["5696686a4bdc2da3298b456a"] },
                    { "5d235b4d86f7742e017bc88a", handbookPrices["5d235b4d86f7742e017bc88a"] }
                }
                );

            return new BSGSuccessBodyResult(packet.ToJson());

        }



        [Route("/client/game/profile/items/moving")]
        [HttpPost]
        public async Task<IActionResult> ItemsMoving()
        {
            var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);

            if (requestBody == null || !requestBody.ContainsKey("data") || requestBody["data"] == null)
            {
                return new BSGErrorBodyResult(400, "Invalid request body");
            }

            JArray commands = JArray.Parse(requestBody["data"].ToString());
            var sessionId = "";
#if !DEBUG
            sessionId = SessionId;
            if (string.IsNullOrEmpty(sessionId))
            {
                Response.StatusCode = 412; // Precondition
                return StatusCode(500);
            }
#else
            sessionId = _saveProvider?.GetProfiles().First().Key;
#endif

            var result = (await (_actionCommandService.ExecuteCommandAsync(commands, sessionId))).ToJson();
            return new BSGSuccessBodyResult(result);
        }

        //private void DoItemsMovingAction_Move(QueueData queueData, JToken actionData)
        //{
        //    var sessionId = SessionId;
        //    var saveProvider = new SaveProvider();
        //    var pmcProfile = saveProvider.GetPmcProfile(sessionId);

        //    var inv = (JToken)pmcProfile["Inventory"];
        //    var invItems = (JArray)inv["items"];
        //    var itemIdToFind = actionData["item"].ToString();
        //    for (var iInvItem = 0; iInvItem < invItems.Count; iInvItem++)
        //    {
        //        var invItem = invItems[iInvItem];
        //        var _id = invItem["_id"].ToString().Trim();
        //        var _tpl = invItem["_tpl"].ToString().Trim();
        //        if (_id == itemIdToFind || _id == itemIdToFind)
        //        {
        //            Debug.WriteLine($"moving {_id} {_tpl}");
        //            var matchedInvItem = invItem;
        //            var m = matchedInvItem["parentId"];// = moveRequest.to.id;

        //            var to = actionData["to"].ToObject<ProcessTo>();
        //            matchedInvItem["slotId"] = to.container;
        //            matchedInvItem["parentId"] = to.id;
        //            if (to.location != null)
        //            {
        //                matchedInvItem["location"] = JToken.Parse(to.location.ToJson());
        //            }
        //            else
        //            {
        //                matchedInvItem["location"] = null;
        //            }
        //            invItems[iInvItem] = matchedInvItem;
        //        }
        //    }
        //    saveProvider.SaveProfile(sessionId);
        //    //saveProvider.SaveProfile(sessionId, pmcProfile);
        //}

        [Route("/client/checkVersion")]
        [HttpPost]
        public IActionResult CheckVersion(int? retry, bool? debug)
        {
            JObject packet = new();
            packet.Add("isValid", true);
            packet.Add("latestVersion", "");
            return new BSGSuccessBodyResult(packet);
        }

        /// <summary>
        /// Arena
        /// </summary>
        [Route("client/profile/presets")]
        [HttpPost]
        public async void ProfilePresets()
        {
            var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);

            var result = new Dictionary<string, object>();
            result.Add("Test", new { id = "test", availableCount = 1, availableUntil = int.MaxValue, experience = 1, isUnlocked = true });

            await HttpBodyConverters.CompressIntoResponseBodyBSG(JsonConvert.SerializeObject(result), Request, Response);
        }

        /// <summary>
        /// Handles the game mode selection for the client and updates the session state.
        /// </summary>
        /// <remarks>This method processes the incoming HTTP POST request to determine the game mode and
        /// backend URL. The game mode is extracted from the request body and stored in the session state. If no game
        /// mode is provided, the default value of "pve" is used. The backend URL is derived from the request host. The
        /// response contains the selected game mode and backend URL.</remarks>
        /// <param name="retry">An optional parameter specifying the number of retry attempts for the operation. If null, no retries are
        /// performed.</param>
        [Route("client/game/mode", Name = "GameMode")]
        [HttpPost]
        public async Task<IActionResult> GameMode()
        {
            var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);

            string protocol = Request?.Protocol?.ToString();
            string ip = Request?.Host.ToString();

            var indexOfSlash3 = Request?.ToString().IndexOf('/', 7);
            string backendUrl = $"https://{ip}/";

            string mode = requestBody != null && requestBody.ContainsKey("sessionMode") && requestBody["sessionMode"] != null ? requestBody["sessionMode"].ToString() : null;
            if (mode == null)
                mode = "pve";

            HttpContext?.Session?.SetString("GameMode", mode);

            _saveProvider?.GetProfiles();
            var account = _saveProvider?.LoadProfile(SessionId);
            if (account != null)
            {
                account.CurrentMode = mode;
                _saveProvider.SaveProfile(SessionId, account);
            }

            // When the game is started in a specific mode, we need to recreate the matching group for that mode. This would be true whenever the game is started or switched modes.
            _matchingService.DeleteMatchingGroupBySessionId(SessionId);
            _matchingService.CreateMatchingGroupBySessionId(SessionId);
            // When the game is started in a specific mode, we need to reset the group invites.
            _saveProvider.GetAccountProfileMode(account).GroupInviteRequests = new();
            // Resave
            _saveProvider.SaveProfile(SessionId, account);

            return new BSGSuccessBodyResult(JObject.FromObject(new { gameMode = mode, backendUrl = ip }));
        }

        [Route("client/builds/list", Name = "BuildsList")]
        [HttpPost]
        public async Task<IActionResult> BuildsList()
        {
            var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);

            JObject obj = new JObject();
            obj.Add("equipmentBuilds", new JArray());
            obj.Add("weaponBuilds", new JArray());
            obj.Add("magazineBuilds", new JArray());

            return new BSGSuccessBodyResult(obj);
        }



        [Route("client/survey", Name = "Survey")]
        [HttpPost]
        public async Task<IActionResult> Survey(int? retry, bool? debug)
        {
            var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);
            return new BSGSuccessBodyResult(new JObject());
        }

        [Route("client/game/logout", Name = "Logout")]
        [HttpPost]
        public async Task<IActionResult> Logout(int? retry, bool? debug)
        {
            var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);

            return new BSGSuccessBodyResult(new { });
        }



        // [Route("client/arena/server/list")]
        // [HttpPost]
        // public async void ArenaServerList(
        //    [FromQuery] int? retry
        //, [FromQuery] bool? debug
        //   )
        // {
        //     // -------------------------------
        //     // ServerItem[]

        //     var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);

        //     var result = Array.Empty<object>();

        //     await HttpBodyConverters.CompressIntoResponseBodyBSG(JsonConvert.SerializeObject(result), Request, Response);
        // }


        [Route("client/game/bot/generate")]
        [HttpPost]
        public async Task<IActionResult> BotGenerate()
        {
            var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);

            var resultArray = new JArray();
            if (requestBody == null)
                return new BSGSuccessBodyResult(resultArray);

            if (!requestBody.ContainsKey("conditions"))
                return new BSGSuccessBodyResult(resultArray);

            var conditions = requestBody["conditions"];

            var strConditions = conditions.ToJson();
#if DEBUG
            Debug.WriteLine(strConditions.ToJson());
#endif

            List<WaveInfoClass> list = JsonConvert.DeserializeObject<List<WaveInfoClass>>(strConditions);

            var bots = new BotGenerationService(_globalsService, _inventoryService).GenerateBots(list);

            ITraceWriter traceWriter = new MemoryTraceWriter();

            var botsJson = JsonConvert.SerializeObject(bots
                , Formatting.Indented
                , new JsonSerializerSettings() { TraceWriter = traceWriter, Converters = DatabaseService.CachedSerializer.Converters }
                );

            //#if DEBUG
            //            Debug.WriteLine(traceWriter);
            //#endif

            return new BSGSuccessBodyResult(botsJson);
        }

        [Route("client/putMetrics")]
        [HttpPost]
        public IActionResult PutMetrics()
        {
            return new BSGSuccessBodyResult(new JObject());
        }

        [Route("client/putHWMetrics")]
        [HttpPost]
        public IActionResult PutHWMetrics()
        {
            return new BSGSuccessBodyResult(new JObject());
        }
    }
}
