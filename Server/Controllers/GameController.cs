using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Paulov.Tarkov.WebServer.DOTNET.Middleware;
using Paulov.Tarkov.WebServer.DOTNET.Providers;
using Paulov.Tarkov.WebServer.DOTNET.ResponseModels;
using Paulov.Tarkov.WebServer.DOTNET.ResponseModels.Survey;
using System.Text.Json;

namespace Paulov.Tarkov.WebServer.DOTNET.Controllers
{
    [ApiController]
    public class GameController : ControllerBase
    {
        private TradingProvider tradingProvider { get; } = new TradingProvider();
        private SaveProvider saveProvider { get; } = new SaveProvider();

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
        public async void Start(int? retry, bool? debug)
        {
            var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);

            var timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0);

            await HttpBodyConverters.CompressDictionaryIntoResponseBodyBSG(
                new Dictionary<string, object>() { { "utc_time", (int)timeSpan.TotalSeconds } }
                , Request, Response);

        }

        [Route("client/game/version/validate")]
        [HttpPost]
        public async void VersionValidate(int? retry, bool? debug)
        {
            await HttpBodyConverters.CompressNullIntoResponseBodyBSG(Request, Response);
        }

        [Route("client/game/config")]
        [Route("client/game/configuration")]
        [HttpPost]
        public async void GameConfig(int? retry, bool? debug)
        {
            var r = Request;
            var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);

            DatabaseProvider.TryLoadLocales(out var locales, out var localesDict, out var languages);

#pragma warning disable SYSLIB0014 // Type or member is obsolete
            string externalIP = Request.Host.ToString(); // Program.publicIp;
            string port = "6969";

            string resolvedIp = $"{externalIP}:{port}";
#pragma warning restore SYSLIB0014 // Type or member is obsolete

            var sessionId = SessionId;
            if (string.IsNullOrEmpty(sessionId))
            {
                Response.StatusCode = 412; // Precondition
                return;
            }

            var profile = saveProvider.LoadProfile(sessionId);
            var pmcProfile = saveProvider.GetPmcProfile(sessionId);
            int aid = int.Parse(profile.Info["aid"].ToString());
            HttpContext.Session.SetInt32("AccountId", aid);

            var config = new Dictionary<string, object>()
            {
                { "languages", languages }
                , { "ndaFree", true }
                , { "reportAvailable", false }
                , { "twitchEventMember", false }
                , { "lang", "en" }
                , { "aid", profile.AccountId }
                , { "taxonomy", 6 }
                , { "activeProfileId", $"{SessionId}" }
                , { "backend",
                    new { Lobby = resolvedIp, Trading = resolvedIp, Messaging = resolvedIp, Main = resolvedIp, Ragfair = resolvedIp }
                }
                , { "useProtobuf", false }
                , { "utc_time", DateTime.UtcNow.Ticks / 1000 }
                , { "totalInGame", 1 }
            };

            await HttpBodyConverters.CompressIntoResponseBodyBSG(config, Request, Response);
        }

        [Route("client/items")]
        [HttpPost]
        [HttpGet]
        public async void TemplateItems(int? retry, bool? debug, int? count, int? page)
        {
            if (DatabaseProvider.TryLoadItemTemplates(out var items, count, page))
                await HttpBodyConverters.CompressIntoResponseBodyBSG(items, Request, Response);
            else
                Response.StatusCode = 500;

        }



        [Route("client/globals")]
        [HttpPost]
        public async void Globals(int? retry, bool? debug)
        {
            // TODO: Detect which Globals to load
            if (DatabaseProvider.TryLoadDatabaseFile("globals.json", out JsonDocument document))
            {
                //if (!items.ContainsKey("LocationInfection"))
                //items.Add("LocationInfection", new { });

                //if (!items.ContainsKey("time"))
                //    items.Add("time", DateTime.Now.Ticks / 1000);

                await HttpBodyConverters.CompressIntoResponseBodyBSG(document.RootElement.GetRawText(), Request, Response);
            }
            else
                Response.StatusCode = 500;
            //if (DatabaseProvider.TryLoadGlobalsArena(out var items))
            //    await HttpBodyConverters.CompressIntoResponseBodyBSG(items, Request, Response);
        }



        [Route("client/settings")]
        [HttpPost]
        public async void Settings(int? retry, bool? debug)
        {
            DatabaseProvider.TryLoadDatabaseFile("settings.json", out Dictionary<string, object> items);

            await HttpBodyConverters.CompressIntoResponseBodyBSG(items, Request, Response);

        }

        [Route("client/game/profile/list")]
        [HttpPost]
        public async void ProfileList(int? retry, bool? debug)
        {
            var sessionId = SessionId;

            var profile = saveProvider.LoadProfile(sessionId);
            if (profile == null)
            {
                Response.StatusCode = 500;
                return;
            }

            var profileInfo = profile.Info as dynamic;
            if (profileInfo != null)
            {
                JArray list = new();
                if ((bool)profileInfo["wipe"])
                {

                }
                else
                {
                    list.Add(saveProvider.GetPmcProfile(sessionId));
                    list.Add(saveProvider.GetScavProfile(sessionId));
                }
                await HttpBodyConverters.CompressIntoResponseBodyBSG(list.ToJson(), Request, Response);
            }


        }

        [Route("client/game/profile/nickname/reserved")]
        [HttpPost]
        public async void NicknameReserved(int? retry, bool? debug)
        {
            await HttpBodyConverters.CompressIntoResponseBodyBSG("\"StayInTarkov\"", Request, Response);

        }

        [Route("client/game/profile/nickname/validate")]
        [HttpPost]
        public async void NicknameValidate(int? retry, bool? debug)
        {
            var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);

            if (requestBody["nickname"].ToString().Length < 3)
            {
                await HttpBodyConverters.CompressIntoResponseBodyBSG(null, Request, Response, 256, "The nickname is too short");
                return;
            }
            //else if (saveProvider.NameExists(requestBody["nickname"].ToString()))
            //{
            //    await HttpBodyConverters.CompressIntoResponseBodyBSG(null, Request, Response, 255, "The nickname is already in use");
            //    return;
            //}

            JObject obj = new();
            obj.TryAdd("status", "ok");
            await HttpBodyConverters.CompressIntoResponseBodyBSG(JsonConvert.SerializeObject(obj), Request, Response);

        }

        [Route("client/game/keepalive")]
        [HttpPost]
        public async void KeepAlive(int? retry, bool? debug)
        {
            JObject obj = new();
            obj.TryAdd("msg", "OK");

            obj.TryAdd("utc_time", DateTime.UtcNow.Ticks / 1000);
            await HttpBodyConverters.CompressIntoResponseBodyBSG(obj, Request, Response);

        }
        [Route("client/account/customization")]
        [HttpPost]
        public async void AccountCustomization(int? retry, bool? debug)
        {
            DatabaseProvider.TryLoadDatabaseFile("templates/character.json", out string items);

            await HttpBodyConverters.CompressIntoResponseBodyBSG(items, Request, Response);

        }



        [Route("client/game/profile/select")]
        [HttpPost]
        public async void ProfileSelect(
            [FromQuery] int? retry
        , [FromQuery] bool? debug
           )
        {
            var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);

            Dictionary<string, dynamic> response = new();
            response.Add("status", "ok");
            try
            {
                Dictionary<string, dynamic> responseNotifier = new NotifierProvider().CreateNotifierPacket(SessionId);
                response.Add("notifier", responseNotifier);
                response.Add("notifierServer", $"{responseNotifier["notifierServer"]}");
            }
            catch (Exception)
            {
                response.Add("notifier", new JObject());
                response.Add("notifierServer", new JObject());
            }
            await HttpBodyConverters.CompressIntoResponseBodyBSG(JsonConvert.SerializeObject(response), Request, Response);
            requestBody = null;
        }

        [Route("client/profile/status")]
        [HttpPost]
        public async void ProfileStatus(
            [FromQuery] int? retry
        , [FromQuery] bool? debug
           )
        {
            var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);

            JObject response = new();
            response.Add("maxPveCountExceeded", false);
            JArray responseProfiles = new();
            JObject profileScav = new();
            profileScav.Add("profileid", $"scav{SessionId}");
            profileScav.Add("profileToken", null);
            profileScav.Add("status", "Free");
            profileScav.Add("sid", $"");
            profileScav.Add("ip", $"");
            profileScav.Add("port", 0);
            profileScav.Add("version", "live");
            profileScav.Add("location", "bigmap");
            profileScav.Add("raidMode", "Online");
            profileScav.Add("mode", "deathmatch");
            profileScav.Add("shortId", "xxx1x1");
            JObject profilePmc = new();
            profilePmc.Add("profileid", $"{SessionId}");
            profilePmc.Add("profileToken", null);
            profilePmc.Add("status", "Free");
            profilePmc.Add("sid", $"");
            profilePmc.Add("ip", $"");
            profilePmc.Add("port", 0);
            responseProfiles.Add(profileScav);
            responseProfiles.Add(profilePmc);
            response.Add("profiles", responseProfiles);

            await HttpBodyConverters.CompressIntoResponseBodyBSG(response, Request, Response);
            requestBody = null;
        }

        [Route("client/locations")]
        [HttpPost]
        public async Task<IActionResult> Locations(
          [FromQuery] int? retry
      , [FromQuery] bool? debug
         )
        {
            if (!DatabaseProvider.TryLoadLocationBases(out var locationJsons))
            {
                Response.StatusCode = 500;
                return new BSGResult(null);
            }

            Dictionary<string, object> response = new();
            response.Add("locations", locationJsons);

            //await HttpBodyConverters.CompressIntoResponseBodyBSG(response, Request, Response);

            return new BSGSuccessBodyResult(response);

        }

        [Route("client/weather")]
        [HttpPost]
        public async void Weather(
          [FromQuery] int? retry
      , [FromQuery] bool? debug
         )
        {
            var requestBody = await HttpBodyConverters.DecompressRequestBody(Request);
            await HttpBodyConverters.CompressIntoResponseBodyBSG(JsonConvert.SerializeObject(new WeatherProvider.WeatherClass()), Request, Response);
        }


        [Route("client/handbook/templates")]
        [HttpPost]
        public async void HandbookTemplates(int? retry, bool? debug)
        {
            DatabaseProvider.TryLoadTemplateFile("handbook.json", out var templates);

            await HttpBodyConverters.CompressIntoResponseBodyBSG(JsonConvert.SerializeObject(templates), Request, Response);

        }

        [Route("client/hideout/areas")]
        [HttpPost]
        public async void HideoutAreas(int? retry, bool? debug)
        {
            DatabaseProvider.TryLoadDatabaseFile(Path.Combine("hideout", "areas.json"), out JArray jobj);

            await HttpBodyConverters.CompressIntoResponseBodyBSG(JsonConvert.SerializeObject(jobj), Request, Response);

        }


        [Route("client/hideout/qte/list")]
        [HttpPost]
        public async void HideoutQTEList(int? retry, bool? debug)
        {
            DatabaseProvider.TryLoadDatabaseFile(Path.Combine("hideout", "qte.json"), out JArray jobj);

            await HttpBodyConverters.CompressIntoResponseBodyBSG(JsonConvert.SerializeObject(jobj), Request, Response);

        }


        [Route("client/hideout/settings")]
        [HttpPost]
        public async void HideoutSettings(int? retry, bool? debug)
        {
            DatabaseProvider.TryLoadDatabaseFile(Path.Combine("hideout", "settings.json"), out JObject jobj);

            await HttpBodyConverters.CompressIntoResponseBodyBSG(JsonConvert.SerializeObject(jobj), Request, Response);

        }

        [Route("client/hideout/production")]
        [Route("client/hideout/production/recipes")]
        [HttpPost]
        public async void HideoutProduction(int? retry, bool? debug)
        {
            DatabaseProvider.TryLoadDatabaseFile(Path.Combine("hideout", "production.json"), out JArray jobj);

            await HttpBodyConverters.CompressIntoResponseBodyBSG(JsonConvert.SerializeObject(jobj), Request, Response);

        }

        [Route("client/hideout/scavcase")]
        [Route("client/hideout/production/scavcase/recipes")]
        [HttpPost]
        public async void HideoutScavcase(int? retry, bool? debug)
        {
            DatabaseProvider.TryLoadDatabaseFile(Path.Combine("hideout", "scavcase.json"), out JArray jobj);

            await HttpBodyConverters.CompressIntoResponseBodyBSG(JsonConvert.SerializeObject(jobj), Request, Response);

        }

        [Route("client/handbook/builds/my/list")]
        [HttpPost]
        public async void UserPresets(int? retry, bool? debug)
        {
            Dictionary<string, object> nullResult = new();
            nullResult.Add("equipmentBuilds", new JArray());
            nullResult.Add("weaponBuilds", new JArray());
            await HttpBodyConverters.CompressIntoResponseBodyBSG(JsonConvert.SerializeObject(nullResult), Request, Response);

        }

        [Route("client/notifier/channel/create")]
        [HttpPost]
        public IActionResult NotifierChannelCreate(int? retry, bool? debug)
        {
            return new BSGSuccessBodyResult(new NotifierProvider().CreateNotifierPacket(SessionId));

        }



        //
        //
        //


        [Route("client/mail/dialog/list")]
        [HttpPost]
        public async void MailDialogList(int? retry, bool? debug)
        {
            await HttpBodyConverters.CompressIntoResponseBodyBSG(JsonConvert.SerializeObject(new JArray()), Request, Response);
        }

        [Route("client/trading/customization/storage")]
        [HttpPost]
        public async void CustomizationStorage(int? retry, bool? debug)
        {
            Dictionary<string, object> packetResult = new();
            packetResult.Add("_id", $"{SessionId}");
            packetResult.Add("suites", saveProvider.LoadProfile(SessionId).Suits);
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
        public async void FriendList(int? retry, bool? debug)
        {
            Dictionary<string, object> packet = new();
            packet.Add("Friends", new JArray());
            packet.Add("Ignore", new JArray());
            packet.Add("InIgnoreList", new JArray());
            await HttpBodyConverters.CompressIntoResponseBodyBSG(JsonConvert.SerializeObject(packet), Request, Response);
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
        public async void GroupCurrent(int? retry, bool? debug)
        {
            JObject packet = new();
            packet.Add("squad", new JArray());
            await HttpBodyConverters.CompressIntoResponseBodyBSG(packet, Request, Response);
        }

        [Route("client/quest/list")]
        [HttpPost]
        public async void QuestList(int? retry, bool? debug)
        {
            await HttpBodyConverters.CompressIntoResponseBodyBSG(JsonConvert.SerializeObject(new JArray()), Request, Response);
        }

        [Route("client/repeatalbeQuests/activityPeriods")]
        [HttpPost]
        public async void RepeatableQuestList(int? retry, bool? debug)
        {
            await HttpBodyConverters.CompressIntoResponseBodyBSG(JsonConvert.SerializeObject(new JArray()), Request, Response);
        }

        [Route("player/health/sync")]
        [HttpPost]
        public async void HealthSync(int? retry, bool? debug)
        {
            await HttpBodyConverters.CompressIntoResponseBodyBSG(JsonConvert.SerializeObject(new JObject()), Request, Response);
        }

        [Route("/client/items/prices/{traderId}")]
        [HttpPost]
        public async void ItemPricesForTraderId(int? retry, bool? debug)
        {
            var tradingProvider = new TradingProvider();
            Dictionary<string, int> handbookPrices = tradingProvider.GetStaticPrices();
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
            await HttpBodyConverters.CompressIntoResponseBodyBSG(JsonConvert.SerializeObject(packet), Request, Response);
        }



        [Route("/client/game/profile/items/moving")]
        [HttpPost]
        public async Task<IActionResult> ItemsMoving(int? retry, bool? debug)
        {
            var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);

            TradingBackend.QueueData queueData = new();
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
            return new BSGSuccessBodyResult(queueData);
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
        public async void CheckVersion(int? retry, bool? debug)
        {
            Dictionary<string, object> packet = new();
            packet.Add("isValid", true);
            packet.Add("latestVersion", "");
            await HttpBodyConverters.CompressIntoResponseBodyBSG(JsonConvert.SerializeObject(packet), Request, Response);
        }

        [Route("client/profile/presets")]
        [HttpPost]
        public async void ProfilePresets(
           [FromQuery] int? retry
       , [FromQuery] bool? debug
          )
        {
            var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);

            var result = new Dictionary<string, object>();
            result.Add("Test", new { id = "test", availableCount = 1, availableUntil = int.MaxValue, experience = 1, isUnlocked = true });

            await HttpBodyConverters.CompressIntoResponseBodyBSG(JsonConvert.SerializeObject(result), Request, Response);
        }



        [Route("client/game/mode", Name = "GameMode")]
        [HttpPost]
        public async void GameMode(int? retry, bool? debug)
        {
            var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);


            //string protocol = "http://";
            // TODO: Add Public IP workarounds from SIT
            string externalIP = "192.168.0.35"; // Program.publicIp;
            string port = "6969";

            string resolvedIp = $"{externalIP}:{port}";
            await HttpBodyConverters.CompressDictionaryIntoResponseBodyBSG(
                new Dictionary<string, object>() { { "gameMode", "regular" }, { "backendUrl", resolvedIp } }
                , Request, Response);

        }

        [Route("client/builds/list", Name = "BuildsList")]
        [HttpPost]
        public async void BuildsList(int? retry, bool? debug)
        {
            var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);

            string protocol = "http://";
            // TODO: Add Public IP workarounds from SIT
            string externalIP = "192.168.0.35"; // Program.publicIp;
            string port = "6969";

            string resolvedIp = $"{protocol}{externalIP}:{port}";
            await HttpBodyConverters.CompressDictionaryIntoResponseBodyBSG(
                new Dictionary<string, object>() { { "equipmentBuilds", new object[0] }, { "weaponBuilds", new object[0] }, { "magazineBuilds", new object[0] } }
                , Request, Response);

        }

        [Route("client/achievement/statistic", Name = "AchievementStat")]
        [HttpPost]
        public async void AchievementStat(int? retry, bool? debug)
        {
            var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);

            await HttpBodyConverters.CompressDictionaryIntoResponseBodyBSG(
                new Dictionary<string, object>() { { "elements", new { } } }
                , Request, Response);

        }

        [Route("client/achievement/list", Name = "AchievementList")]
        [HttpPost]
        public async void AchievementList(int? retry, bool? debug)
        {
            var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);

            await HttpBodyConverters.CompressDictionaryIntoResponseBodyBSG(
                new Dictionary<string, object>() { { "elements", new object[0] } }
                , Request, Response);

        }

        [Route("client/survey", Name = "Survey")]
        [HttpPost]
        public async void Survey(int? retry, bool? debug)
        {
            var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);

            await HttpBodyConverters.CompressIntoResponseBodyBSG(
                new SurveyResponseModel()
                , Request, Response);

        }

        [Route("client/game/logout", Name = "Logout")]
        [HttpPost]
        public async void Logout(int? retry, bool? debug)
        {
            var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);

            await HttpBodyConverters.CompressIntoResponseBodyBSG(
                new { }
                , Request, Response);

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
    }
}
