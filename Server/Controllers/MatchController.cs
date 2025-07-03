using BSGHelperLibrary.ResponseModels;
using EFT;
using JsonType;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Paulov.Tarkov.WebServer.DOTNET.Middleware;
using Paulov.TarkovModels;
using Paulov.TarkovModels.Responses;
using Paulov.TarkovServices.Providers.Interfaces;
using Paulov.TarkovServices.Services;
using Paulov.TarkovServices.Services.Interfaces;
using System.Diagnostics;

namespace Paulov.Tarkov.WebServer.DOTNET.Controllers
{
    public class MatchController : ControllerBase
    {
        private ISaveProvider _saveProvider;
        private IInventoryService _inventoryService;
        private IGlobalsService _globalsService;
        private IAccountService _accountService;
        private IWebSocketService _webSocketService;

        public MatchController(ISaveProvider saveProvider, IInventoryService inventoryService, IGlobalsService globalsService, IAccountService accountService, IWebSocketService webSocketService)
        {
            _saveProvider = saveProvider;
            _inventoryService = inventoryService;
            _globalsService = globalsService;
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
            _webSocketService = webSocketService ?? throw new ArgumentNullException(nameof(webSocketService));
        }

        private string SessionId
        {
            get
            {
                return HttpSessionHelpers.GetSessionId(Request, HttpContext);
            }
        }

        [Route("client/match/group/current")]
        [HttpPost]
        public async Task<IActionResult> MatchingGroupCurrent(int? retry, bool? debug)
        {
            var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);

            var account = _saveProvider.LoadProfile(SessionId);
            if (account == null)
            {
                return new BSGErrorBodyResult(500, "Account not found");
            }

            if (_saveProvider.GetAccountProfileMode(account).MatchingGroup == null)
                _saveProvider.GetAccountProfileMode(account).MatchingGroup = new MatchingGroup()
                {
                    MatchingGroupId = MongoID.Generate(false).ToString(),
                    Members = new List<string>() { account.AccountId },
                    SquadLeaderId = account.AccountId
                };

            var matchGroup = _saveProvider.GetAccountProfileMode(account).MatchingGroup;

            var members = new JArray();
            foreach (var memberId in matchGroup.Members)
            {
                members.Add(JObject.FromObject(_accountService.GetMatchingGroupMember(_saveProvider.LoadProfile(memberId), memberId == SessionId, false, null)));
            }

            JObject packet = new();
            packet.Add("squad", members);
            packet.Add("raidSettings", new JObject());

            return new BSGSuccessBodyResult(packet);
        }

        [Route("client/match/group/invite/send")]
        [HttpPost]
        public async Task<IActionResult> MatchGroupingInviteSend()
        {
            var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);
            if (requestBody == null)
                return new BSGErrorBodyResult(500, "");

            var fromAccount = _saveProvider.LoadProfile(SessionId);
            if (fromAccount == null)
            {
                return new BSGErrorBodyResult(500, "Own account not found");
            }

            var toAccountId = requestBody["to"].ToString();
            var inLobby = bool.Parse(requestBody["inLobby"].ToString());

            var toAccount = _accountService.GetAccountByAID(toAccountId);
            if (toAccount == null)
            {
                return new BSGErrorBodyResult(500, "Other account not found");
            }

            // if user is not logged in. send not logged in error.
            if (_webSocketService.GetWebSocket(toAccount.AccountId) == null)
            {
                return new BSGErrorBodyResult(502014, "Player is not Online");
            }

            if (_saveProvider.GetAccountProfileMode(fromAccount).MatchingGroup == null)
            {
                _saveProvider.GetAccountProfileMode(fromAccount).MatchingGroup = new MatchingGroup()
                {
                    MatchingGroupId = MongoID.Generate(false).ToString(),
                    Members = new List<string>() { fromAccount.AccountId, toAccountId }
                };
            }

            if (_saveProvider.GetAccountProfileMode(toAccount).MatchingGroup == null)
                _saveProvider.GetAccountProfileMode(toAccount).MatchingGroup = new MatchingGroup();

            var matchGroup = _saveProvider.GetAccountProfileMode(fromAccount).MatchingGroup;
            matchGroup.SquadLeaderId = fromAccount.AccountId;

            if (!matchGroup.Members.Any(x => x == fromAccount.AccountId))
                matchGroup.Members.Add(fromAccount.AccountId);

            if (!matchGroup.Members.Any(x => x == toAccount.AccountId))
                matchGroup.Members.Add(toAccount.AccountId);

            _saveProvider.GetAccountProfileMode(toAccount).MatchingGroup = matchGroup;
            _saveProvider.GetAccountProfileMode(fromAccount).MatchingGroup = matchGroup;

            var members = new JArray();

            foreach (var memberId in matchGroup.Members)
            {
                members.Add(JObject.FromObject(_accountService.GetMatchingGroupMember(_saveProvider.LoadProfile(memberId), memberId == SessionId, inLobby, null)));
            }

            var requestId = MongoID.Generate(false).ToString();
            _webSocketService.SendNotificationToWebSocket(toAccount.AccountId, EFT.Communications.ENotificationType.GroupMatchInviteSend, new JObject()
            {
                { "eventId", requestId },
                { "requestId", requestId },
                { "from", _saveProvider.GetPmcProfile(fromAccount).AccountId.ToString() },
                { "members", members },
                { "isLeader", true },
                { "isReady", inLobby },
            });

            _saveProvider.SaveProfile(toAccount.AccountId, toAccount);
            _saveProvider.SaveProfile(fromAccount.AccountId, fromAccount);

            return new BSGSuccessBodyResult(requestId);
        }

        [Route("client/match/group/invite/accept")]
        [HttpPost]
        public async Task<IActionResult> MatchGroupingInviteAccept()
        {
            var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);
            if (requestBody == null)
                return new BSGErrorBodyResult(500, "");

            var fromAccount = _saveProvider.LoadProfile(SessionId);
            if (fromAccount == null)
            {
                return new BSGErrorBodyResult(500, "Own account not found");
            }

            var group = _saveProvider.GetAccountProfileMode(fromAccount).MatchingGroup;

            var squadMembers = group.Members.Select(memberId => _accountService.GetMatchingGroupMember(_saveProvider.LoadProfile(memberId), memberId == group.SquadLeaderId, false, null));

            foreach (var member in squadMembers)
            {
                _webSocketService.SendNotificationToWebSocket(member.Id, EFT.Communications.ENotificationType.GroupMatchInviteAccept, JObject.FromObject(member));
            }

            return new BSGSuccessBodyResult(JArray.FromObject(squadMembers));
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
            DatabaseService.TryLoadLocationBases(out JObject locationsJO);

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
            DatabaseService.TryLoadDatabaseFile("templates/locationServices.json", out JObject serverSettings);
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
            var requestBody = await HttpBodyConverters.DecompressRequestBodyToString(Request);
            if (requestBody == null)
                return new BSGErrorBodyResult(500, "");

#if DEBUG
            if (
               (Request.Headers.ContainsKey("Content-Encoding") && Request.Headers["Content-Encoding"] == "deflate")
               || (Request.Headers.ContainsKey("user-agent") && Request.Headers["user-agent"].ToString().StartsWith("Unity"))
               )
            {

                System.IO.File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "DumpMatchLocalEnd.json"), requestBody);
            }
#endif

            _globalsService.LoadGlobalsIntoComfortSingleton();

            ITraceWriter traceWriter = new MemoryTraceWriter();

            var obj = JsonConvert.DeserializeObject<JObject>(
                requestBody
                , new JsonSerializerSettings() { Converters = DatabaseService.CachedSerializer.Converters, NullValueHandling = NullValueHandling.Ignore, StringEscapeHandling = StringEscapeHandling.EscapeNonAscii, TraceWriter = traceWriter });

            //#if DEBUG
            //            Debug.WriteLine(traceWriter);
            //#endif

            obj.TryGetValue("results", out var results);
            JObject resultsJO = ((JObject)results);
            resultsJO.TryGetValue("profile", out var profileToken);

            AccountProfileCharacter matchEndProfile = null;

            // Wipe the Hideout Seed. This is a workaround for the issue where the Hideout Seed is not correct after a local match ends.
            profileToken["Hideout"]["Seed"] = null;
            var profileJson = profileToken.ToString(Formatting.Indented, DatabaseService.CachedSerializer.Converters.ToArray());
            try
            {

                // create a new AccountProfileCharacter from the profileToken
                matchEndProfile = JsonConvert.DeserializeObject<AccountProfileCharacter>(
                    profileJson
                    , new JsonSerializerSettings() { TraceWriter = traceWriter, Converters = DatabaseService.CachedSerializer.Converters });
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            if (matchEndProfile == null)
            {
                Debug.WriteLine("Match End Profile is null, this should not happen!");
                return new BSGErrorBodyResult(500, "Match End Profile is null, this should not happen!");
            }

            var matchResult = resultsJO.GetValue("result").ToString();
            var isKilled = matchResult == "Killed";

            var localMatchResponse = new LocalMatchEndResponse();
            localMatchResponse.ServerId = MongoID.Generate(false).ToString();

            var myAccount = _saveProvider.LoadProfile(matchEndProfile.Id);
            var myAccountByMode = _saveProvider.GetAccountProfileMode(myAccount);

            var isPMC = myAccountByMode.Characters.PMC.Id == matchEndProfile.Id;

            if (isPMC)
                myAccountByMode.Characters.PMC.Info.Experience = matchEndProfile.Info.Experience;
            else
                myAccountByMode.Characters.Scav.Info.Experience = matchEndProfile.Info.Experience;

            if (isPMC)
                myAccountByMode.Characters.PMC.InsuredItems = matchEndProfile.InsuredItems;

            if (isPMC)
            {
                var currentProfileItems = _inventoryService.GetInventoryItems(myAccountByMode.Characters.PMC).ToList();
                foreach (var item in _inventoryService.GetInventoryItems(matchEndProfile))
                {
                    if (currentProfileItems.FindIndex(x => x._id == item._id) == -1)
                    {
                        // Add the item to the PMC inventory
                        _inventoryService.AddItemToInventory(myAccountByMode.Characters.PMC, item);
                    }
                    else
                    {
                        // Replace the item in the PMC inventory
                        _inventoryService.RemoveItemAndChildItemsFromProfile(myAccountByMode.Characters.PMC, item._id);
                        _inventoryService.AddItemToInventory(myAccountByMode.Characters.PMC, item);
                    }
                }
            }

            if (isKilled && isPMC)
            {
                _inventoryService.RemoveItemFromSlot(myAccountByMode.Characters.PMC, "Headwear");

                _inventoryService.RemoveItemFromSlot(myAccountByMode.Characters.PMC, "Eyewear");

                _inventoryService.RemoveItemFromSlot(myAccountByMode.Characters.PMC, "FaceCover");

                _inventoryService.RemoveItemFromSlot(myAccountByMode.Characters.PMC, "Earpiece");

                _inventoryService.RemoveItemFromSlot(myAccountByMode.Characters.PMC, "ArmorVest");

                _inventoryService.RemoveItemFromSlot(myAccountByMode.Characters.PMC, "TacticalVest");

                _inventoryService.RemoveItemFromSlot(myAccountByMode.Characters.PMC, "Backpack");

                _inventoryService.RemoveItemFromSlot(myAccountByMode.Characters.PMC, "pocket1");
                _inventoryService.RemoveItemFromSlot(myAccountByMode.Characters.PMC, "pocket2");
                _inventoryService.RemoveItemFromSlot(myAccountByMode.Characters.PMC, "pocket3");
                _inventoryService.RemoveItemFromSlot(myAccountByMode.Characters.PMC, "pocket4");

                _inventoryService.RemoveItemFromSlot(myAccountByMode.Characters.PMC, "FirstPrimaryWeapon");

                _inventoryService.RemoveItemFromSlot(myAccountByMode.Characters.PMC, "SecondPrimaryWeapon");

                _inventoryService.RemoveItemFromSlot(myAccountByMode.Characters.PMC, "Holster");
            }

            if (isPMC)
            {
                myAccountByMode.Characters.PMC.Encyclopedia = matchEndProfile.Encyclopedia;
                myAccountByMode.Characters.PMC.Health = matchEndProfile.Health;
                myAccountByMode.Characters.PMC.QuestsData = matchEndProfile.QuestsData;
                myAccountByMode.Characters.PMC.Skills = matchEndProfile.Skills;
                myAccountByMode.Characters.PMC.Stats = matchEndProfile.Stats;
                myAccountByMode.Characters.PMC.TaskConditionCounters = matchEndProfile.TaskConditionCounters;
            }

            localMatchResponse.Results = ((JObject)results);

            _saveProvider.SaveProfile(matchEndProfile.Id, myAccount);
            Debug.WriteLine("Match End Profile saved successfully.");

            return new BSGSuccessBodyResult(localMatchResponse.ToJson());
        }

        [Route("client/getMetricsConfig")]
        [HttpPost]
        public async Task<IActionResult> GetMetricsConfig()
        {
            DatabaseService.TryLoadDatabaseFile("match/metrics.json", out JObject dbFile);
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
