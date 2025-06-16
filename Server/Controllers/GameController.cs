using BSGHelperLibrary.ResponseModels;
using JsonType;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Paulov.Tarkov.WebServer.DOTNET.Middleware;
using Paulov.TarkovServices;
using Paulov.TarkovServices.Providers.Interfaces;
using Paulov.TarkovServices.Providers.SaveProviders;
using Paulov.TarkovServices.Services;
using System.Diagnostics;

namespace Paulov.Tarkov.WebServer.DOTNET.Controllers
{
    [ApiController]
    public class GameController : ControllerBase
    {
        private TradingProvider tradingProvider { get; } = new TradingProvider();

        private JsonFileSaveProvider _saveProvider;
        private IConfiguration configuration;
        public GameController(ISaveProvider saveProvider, IConfiguration configuration)
        {
            this._saveProvider = saveProvider as JsonFileSaveProvider;
            this.configuration = configuration;
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

            if (DatabaseProvider.TryLoadItemTemplates(out var items, count, page))
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
            var globals = GlobalsService.Instance.LoadGlobalsIntoComfortSingleton();
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
            DatabaseProvider.TryLoadDatabaseFile("settings.json", out JObject items);

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
            DatabaseProvider.TryLoadDatabaseFile("templates/character.json", out string items);

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
            locationWeatherTime.SeasonsSettings = new GClass2448();
            Debug.WriteLine(locationWeatherTime.ToJson());

            return new BSGSuccessBodyResult(locationWeatherTime.ToJson());
        }


        [Route("client/handbook/templates")]
        [HttpPost]
        public async Task<IActionResult> HandbookTemplates(int? retry, bool? debug)
        {
            DatabaseProvider.TryLoadTemplateFile("handbook.json", out var templates);

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

        [Route("client/friend/request/list/inbox")]
        [HttpPost]
        public async void FriendRequestInbox(int? retry, bool? debug)
        {
            await HttpBodyConverters.CompressIntoResponseBodyBSG(JsonConvert.SerializeObject(new JArray()), Request, Response);
        }

        [Route("client/friend/request/list/outbox")]
        [HttpPost]
        public async void FriendRequestOutbox(int? retry, bool? debug)
        {
            await HttpBodyConverters.CompressIntoResponseBodyBSG(JsonConvert.SerializeObject(new JArray()), Request, Response);
        }

        [Route("client/friend/list")]
        [HttpPost]
        public async Task<IActionResult> FriendList(int? retry, bool? debug)
        {
            JObject packet = new();
            packet.Add("Friends", new JArray());
            packet.Add("Ignore", new JArray());
            packet.Add("InIgnoreList", new JArray());
            return new BSGSuccessBodyResult(packet);
        }

        [Route("client/server/list")]
        [HttpPost]
        public async void ServerList(int? retry, bool? debug)
        {
            var packets = new List<Dictionary<string, object>>();
            await HttpBodyConverters.CompressIntoResponseBodyBSG(JsonConvert.SerializeObject(packets), Request, Response);
        }

        [Route("client/match/group/current")]
        [HttpPost]
        public async Task<IActionResult> GroupCurrent(int? retry, bool? debug)
        {
            JObject packet = new();
            packet.Add("squad", new JArray());
            //packet.Add("raidSettings", new JObject());

            return new BSGSuccessBodyResult(packet);
        }

        [Route("client/quest/list")]
        [HttpPost]
        public async Task<IActionResult> QuestList(int? retry, bool? debug)
        {
            //await HttpBodyConverters.CompressIntoResponseBodyBSG(JsonConvert.SerializeObject(new JArray()), Request, Response);

            return new BSGSuccessBodyResult(new JArray());
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
            var tradingProvider = new TradingProvider();
            JObject handbookPrices = JObject.Parse(tradingProvider.GetStaticPrices().ToJson());
            Dictionary<string, object> packet = new();
            packet.Add("supplyNextTime", 0);
            packet.Add("prices", handbookPrices);
            packet.Add("currencyCourses",
                new Dictionary<string, object>() {
                    { "5449016a4bdc2d6f028b456f", handbookPrices["5449016a4bdc2d6f028b456f"] },
                    {  "569668774bdc2da2298b4568", handbookPrices["569668774bdc2da2298b4568"] },
                    { "5696686a4bdc2da3298b456a", handbookPrices["5696686a4bdc2da3298b456a"] }
                }
                );

            return new BSGSuccessBodyResult(handbookPrices);

        }



        [Route("/client/game/profile/items/moving")]
        [HttpPost]
        public async Task<IActionResult> ItemsMoving(int? retry, bool? debug)
        {
            var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);

            //TradingBackend.QueueData queueData = new();
            //queueData.ProfileChanges = new();
            //queueData.InventoryWarnings = Array.Empty<TradingBackend.InventoryWarning>();

            ////try
            ////{
            ////    var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);
            ////    var sessionId = SessionId;
            ////    var saveProvider = new SaveProvider();
            //var pmcProfile = saveProvider.GetPmcProfile(SessionId);


            //if (!queueData.ProfileChanges.ContainsKey(SessionId))
            //    queueData.ProfileChanges.Add(SessionId, new Changes()
            //    {
            //        Experience = 0,
            //        HideoutAreaStashes = new Dictionary<EFT.EAreaType, EFT.HideoutAreaStashInfo>(),
            //        //Production = new Dictionary<string, EFT.Hideout.ProductionData>(),
            //        Quests = Array.Empty<RawQuestClass>(),
            //        RagFairOffers = new EFT.UI.Ragfair.Offer[0],
            //        RepeatableQuests = Array.Empty<DailyQuestClass>(),
            //        Stash = new StashChanges() { change = new FlatItem[0], del = new FlatItem[0], @new = new FlatItem[0] },
            //        TradersData = new Dictionary<string, EFT.TraderData>(),
            //        UnlockedRecipes = new Dictionary<string, bool>()
            //    });



            //JArray data = (JArray)requestBody["data"];
            //foreach (var actionData in data)
            //{
            //    var action = actionData["Action"].ToString();

            //    string type = null;
            //    if (actionData["type"] != null)
            //        type = actionData["type"].ToString();

            //    JToken item;
            //    if (actionData["item"] != null)
            //        item = actionData["item"];

            //    JToken to;
            //    if (actionData["to"] != null)
            //        to = actionData["to"];

            //    IEnumerable<JToken> items = null;
            //    if (actionData["items"] != null)
            //        items = actionData["items"].ToArray();

            //    switch (action)
            //    {
            //        case "Move":
            //            //DoItemsMovingAction_Move(queueData, actionData);

            //            break;
            //        // Buying Selling from Trader
            //        case "TradingConfirm":
            //            if (items == null)
            //                break;

            //            switch (type)
            //            {
            //                case "sell_to_trader":

            //                    var processSellTradeData = actionData.ToObject<ProcessSellTradeRequestData>();

            //                    //queueData.ProfileChanges.Add(new MongoID(true), new Changes() { Stash = new StashChanges() { del = new List<Items>() } });

            //                    for (var iIt = 0; iIt < processSellTradeData.items.Count(); iIt++)
            //                    //foreach (var it in processSellTradeData.items)
            //                    {
            //                        var it = processSellTradeData.items[iIt];
            //                        var itemIdToFind = it.id.Trim();
            //                        var inv = (JToken)pmcProfile["Inventory"];
            //                        var invItems = (JArray)inv["items"];
            //                        //foreach (var invItem in invItems)
            //                        var deletedItemsCount = 0;
            //                        for (var iInvItem = 0; iInvItem < invItems.Count; iInvItem++)
            //                        {
            //                            var invItem = invItems[iInvItem];
            //                            var _id = invItem["_id"].ToString().Trim();
            //                            var _tpl = invItem["_tpl"].ToString().Trim();
            //                            if (_id == itemIdToFind || _id == itemIdToFind)
            //                            {
            //                                Debug.WriteLine($"selling {_id} {_tpl}");

            //                                queueData.ProfileChanges[SessionId].Stash = new StashChanges()
            //                                {
            //                                    @new = new FlatItem[0],
            //                                    change = new FlatItem[0],
            //                                    del = new FlatItem[processSellTradeData.items.Length]
            //                                };

            //                                var l = new UnparsedData() { JToken = invItem["location"] };
            //                                //var delItem = (new Items() { _id = itemIdToFind, _tpl = _tpl, location = l, parentId = invItem["parentId"].ToString(), slotId = invItem["slotId"].ToString() });
            //                                var delItem = (new FlatItem() { _id = itemIdToFind });
            //                                queueData.ProfileChanges[SessionId].Stash.del[iIt] = delItem;
            //                                deletedItemsCount++;
            //                                if (deletedItemsCount == processSellTradeData.items.Length)
            //                                    break;
            //                            }
            //                        }
            //                    }
            //                    break;
            //                default:
            //                    break;
            //            }

            //            break;
            //        // Buying an Offer from Flea
            //        case "RagFairBuyOffer":
            //            break;
            //        // The Sell All button after a Scav Raid
            //        case "SellAllFromSavage":
            //            break;
            //    }

            //}


            //foreach (var kvpProfileChanges in queueData.ProfileChanges)
            //{
            //    saveProvider.ProcessProfileChanges(kvpProfileChanges.Key, kvpProfileChanges.Value);
            //}


            ////}
            ////catch (Exception e)
            ////{
            ////    Debug.WriteLine(e);
            ////}

            ////var queueDataJson = JsonConvert.SerializeObject(queueData);
            ////await HttpBodyConverters.CompressIntoResponseBodyBSG(queueDataJson, Request, Response);
            return new BSGSuccessBodyResult(null);
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

            //await HttpBodyConverters.CompressDictionaryIntoResponseBodyBSG(
            //    new Dictionary<string, object>() { { "gameMode", mode }, { "backendUrl", ip } }
            //    , Request, Response);

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

            var bots = new BotGenerationService().GenerateBots(list);

            ITraceWriter traceWriter = new MemoryTraceWriter();

            var botsJson = JsonConvert.SerializeObject(bots
                , Formatting.Indented
                , new JsonSerializerSettings() { TraceWriter = traceWriter, Converters = DatabaseProvider.CachedSerializer.Converters }
                );

#if DEBUG
            Debug.WriteLine(traceWriter);
#endif

            return new BSGSuccessBodyResult(botsJson);
        }
    }
}
