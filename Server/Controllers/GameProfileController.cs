using BSGHelperLibrary.ResponseModels;
using ChatShared;
using EFT;
using EFT.Hideout;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Paulov.Tarkov.WebServer.DOTNET.Middleware;
using Paulov.TarkovModels;
using Paulov.TarkovServices.Helpers;
using Paulov.TarkovServices.Providers.Interfaces;
using Paulov.TarkovServices.Services;
using Paulov.TarkovServices.Services.Interfaces;
using System.Text;

namespace Paulov.Tarkov.Web.Api.Controllers
{
    /// <summary>
    /// Provides functionality for managing game profiles, including creating and searching profiles.
    /// </summary>
    /// <remarks>This controller handles operations related to game profiles, such as creating new profiles
    /// and searching for existing ones. It interacts with session data and utilizes a save provider to manage profile
    /// persistence.</remarks>
    [ApiController]
    [Produces("application/json")]
    public class GameProfileController : ControllerBase
    {
        private ISaveProvider _saveProvider;
        private IGlobalsService _globalsService;
        private IAccountService _accountService;
        private IInventoryService _inventoryService;
        private Dictionary<string, MongoID> Voices = new();

        public GameProfileController(ISaveProvider saveProvider, IGlobalsService globalsService, IAccountService accountService, IInventoryService inventoryService)
        {
            _saveProvider = saveProvider;
            _globalsService = globalsService;
            _inventoryService = inventoryService;

            if (DatabaseHelpers.TryLoadDatabaseFile("templates/customization.json", out JObject customizationTemplates))
            {
                foreach (var j in customizationTemplates)
                {
                    var key = j.Key;
                    var value = j.Value;
                    // get the voices from the customization templates (5fc100cf95572123ae738483 is the parent id)
                    if (value["_parent"]?.ToString() == "5fc100cf95572123ae738483")
                    {
                        Voices.Add(value["_name"].ToString(), j.Key);
                    }
                }
            }
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService), "AccountService cannot be null.");
            _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService), "InventoryService cannot be null.");
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

        [Route("client/profile/status")]
        [HttpPost]
        public async Task<IActionResult> ProfileStatus()
        {
            var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);

            var sessionId = SessionId;
#if !DEBUG
            if (string.IsNullOrEmpty(sessionId))
            {
                Response.StatusCode = 412; // Precondition
                return new BSGErrorBodyResult(412, "No Session Found!");
            }
#else
            if (string.IsNullOrEmpty(sessionId))
            {
                sessionId = _saveProvider.GetProfiles().First().Key;
            }

#endif
            var account = _saveProvider.LoadProfile(sessionId);
            var mode = _saveProvider.GetAccountProfileMode(account);

            JObject response = new();
            response.Add("maxPveCountExceeded", false);
            JArray responseProfiles = new();
            ProfileStatusClass profileScav = new() { status = EFT.EProfileStatus.Free };
            profileScav.profileid = mode.Characters.Scav.Id;
            ProfileStatusClass profilePmc = new() { status = EFT.EProfileStatus.Free };
            profilePmc.profileid = mode.Characters.PMC.Id;
            responseProfiles.Add(JObject.FromObject(profileScav));
            responseProfiles.Add(JObject.FromObject(profilePmc));
            response.Add("profiles", responseProfiles);

            return new BSGSuccessBodyResult(response);
        }

        /// <summary>
        /// Create a Profile
        /// </summary>
        /// <returns></returns>
        [Route("client/game/profile/create")]
        [HttpPost]
        public async Task<IActionResult> ProfileCreate()
        {
            var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);

            var gameMode = HttpContext.Session != null && HttpContext.Session.GetString("GameMode") != null ? HttpContext.Session.GetString("GameMode") : "pve";

            var sessionId = SessionId;
#if DEBUG
            if (sessionId == null)
            {
                sessionId = _saveProvider.CreateAccount(new AccountCreationModel() { Username = "un", Password = "un", Edition = "Edge_Of_Darkness" });
            }
#endif

            if (string.IsNullOrEmpty(sessionId))
            {
                Response.StatusCode = 412; // Precondition
                return new BSGErrorBodyResult(412, "No Session Found!");
            }

            var account = _saveProvider.LoadProfile(sessionId);
            if (account == null)
            {
#if DEBUG
                sessionId = _saveProvider.GetProfiles().Any() ? _saveProvider.GetProfiles().Keys.First() : MongoID.Generate(false);
                // if we are running from Swagger and havent "logged in". just get this here
                account = _saveProvider.LoadProfile(sessionId);

                if (account == null)
                {
                    sessionId = _saveProvider.CreateAccount(new Dictionary<string, object>() { { "username", "Swagger" }, { "password", "Swagger" }, { "edition", "Edge Of Darkness" } });
                    account = _saveProvider.LoadProfile(sessionId);
                }
#else
                Response.StatusCode = 500;
                return new BSGErrorBodyResult(500, "No Session Found!");
#endif
            }

            if (requestBody == null || !requestBody.ContainsKey("side"))
            {
                if (requestBody == null)
                    requestBody = new Dictionary<string, object>();

                requestBody.Add("side", "usec");
                requestBody.Add("nickname", "Swagger");
                requestBody.Add("headId", "60a6aaad42fd2735e4589978");
                requestBody.Add("voiceId", "5fc615110b735e7b024c76ea");
            }

            _globalsService.LoadGlobalsIntoComfortSingleton();

            var profileModels = DatabaseHelpers.GetObject<ProfileEditionModels>("templates/profiles.json");

            //if (!DatabaseHelpers.TryLoadDatabaseFile("templates/profiles.json", out JObject profileTemplates))
            if (!DatabaseHelpers.TryGetJObject("templates/profiles.json", out JObject profileTemplates))
            {
                Response.StatusCode = 500;
                return new BSGErrorBodyResult(500, "");
            }

            if (!DatabaseHelpers.TryLoadDatabaseFile("templates/customization.json", out JObject customizationTemplates))
            {
                Response.StatusCode = 500;
                return new BSGErrorBodyResult(500, "");
            }

            var blankStat = new ProfileStatsDescriptor()
            {
                CarriedQuestItems = new List<MongoID>(),
                DamageHistory = new DamageHistoryDescriptor() { LethalDamagePart = EBodyPart.Head, BodyParts = new Dictionary<EBodyPart, BodyPartDamageHistoryDescriptor>() },
                DroppedItems = new List<DroppedItem>(),
                ExperienceBonusMult = 0,
                FoundInRaidItems = new List<FoundInRaidItem>(),
                LastPlayerState = null,
                SessionCounters = new CounterCollectionDescriptor(),
                OverallCounters = new CounterCollectionDescriptor(),
                SessionExperienceMult = 0,
                SurvivorClass = ProfileStats.ESurvivorClass.Unknown,
                TotalInGameTime = 0,
                TotalSessionExperience = 0,
                Victims = new List<VictimStats>()
            };
            var blankStatGroup = new ProfileStatsSeparatorDescriptor()
            {
                Eft = blankStat.Clone()
                ,
                Arena = blankStat.Clone()
            };

            var template = profileTemplates[(string)account.Edition][requestBody["side"].ToString().ToLower()]["character"];


            // TODO: Use pmcData2 system
            var pmcData2 = profileModels.Edge_Of_Darkness.Usec.Character;
            switch (account.Edition)
            {
                case "Standard":
                    switch (requestBody["side"].ToString().ToLower())
                    {
                        case "bear":
                            pmcData2 = profileModels.Standard.Bear.Character;
                            break;
                        case "usec":
                            pmcData2 = profileModels.Standard.Usec.Character;
                            break;
                    }
                    break;
                case "Unheard":
                    switch (requestBody["side"].ToString().ToLower())
                    {
                        case "bear":
                            pmcData2 = profileModels.Unheard.Bear.Character;
                            break;
                        case "usec":
                            pmcData2 = profileModels.Unheard.Usec.Character;
                            break;
                    }
                    break;
                default:
                    switch (requestBody["side"].ToString().ToLower())
                    {
                        case "bear":
                            pmcData2 = profileModels.Edge_Of_Darkness.Bear.Character;
                            break;
                        case "usec":
                            pmcData2 = profileModels.Edge_Of_Darkness.Usec.Character;
                            break;
                    }
                    break;
            }


            var accountIdNumber = (new Random().Next(100000, 500000));
            var accountIdString = accountIdNumber.ToString();

            // TODO: Use pmcData2 system
            pmcData2.Customization[EBodyModelPart.Head] = requestBody["headId"].ToString();
            pmcData2.AccountId = accountIdString;
            pmcData2.Id = sessionId;
            pmcData2.Info.Nickname = requestBody["nickname"].ToString();
            pmcData2.Info.RegistrationDate = new Random().Next(100000, 500000);
            pmcData2.Customization[EBodyModelPart.Voice] = requestBody["voiceId"].ToString();
            pmcData2.Stats = blankStatGroup;
            pmcData2.WishList = new Dictionary<MongoID, byte>();
            pmcData2.Info.MemberCategory = EMemberCategory.Default;
            pmcData2.Info.SelectedMemberCategory = EMemberCategory.Default;
            // TODO: Remap GClass2034 to HideoutAreaDescriptor
            pmcData2.Hideout.Areas = template["Hideout"]["Areas"].ToObject<AreaInfo[]>();
            pmcData2.Hideout.GlobalCustomization = template["Hideout"]["Customization"].ToObject<Dictionary<EHideoutCustomizationType, MongoID?>>();
            //pmcData2.Hideout = new HideoutDescriptor();

            template["Customization"]["Head"] = requestBody["headId"].ToString();
            template["Customization"]["Voice"] = requestBody["voiceId"].ToString();
            template["_id"] = sessionId;
            template["aid"] = accountIdString;
            template["savage"] = null;
            template["Info"]["Nickname"] = requestBody["nickname"].ToString();
            template["Info"]["LowerNickname"] = requestBody["nickname"].ToString().ToLower();
            template["Info"]["RegistrationDate"] = new Random().Next(100000, 500000);
            template["Stats"] = JToken.FromObject(blankStatGroup);
            template["WishList"] = JToken.FromObject(new Dictionary<MongoID, byte>());
            template["Hideout"]["Seed"] = "";
            var hideoutCheck = template["Hideout"];

            var templateJsonString = JsonConvert.SerializeObject(template, DatabaseHelpers.GetJsonSerializerSettings());

            // Get Template Profile
            //var pmcData = template.ToObject<AccountProfileCharacter>();
            var pmcData = JsonConvert.DeserializeObject<AccountProfileCharacter>(templateJsonString, DatabaseHelpers.GetJsonSerializerSettings());
            if (pmcData == null)
            {
                Response.StatusCode = 500;
                return new BSGErrorBodyResult(500, "");
            }

            pmcData.Info.MemberCategory = EMemberCategory.Default;
            pmcData.Info.SelectedMemberCategory = EMemberCategory.Default;

            if (gameMode != null)
                account.CurrentMode = gameMode;

            // Create scav -------------------------------------------------------------------------------------------
            var scavData = new BotGenerationService(_globalsService, _inventoryService).GenerateBot(new WaveInfoClass(1, WildSpawnType.assault, BotDifficulty.normal));
            scavData.Id = MongoID.Generate();
            pmcData.PetId = scavData.Id;

            // Assign the profiles -----------------------------------------------------------------------------------
            _saveProvider.GetAccountProfileMode(account).Characters.PMC = pmcData;
            _saveProvider.GetAccountProfileMode(account).Characters.Scav = scavData;

            _saveProvider.SaveProfile(sessionId, account);

            requestBody = null;

            // FYI: The result doesn't mean anything to the Tarkov client. This is just for Swagger / Testing purposes
            var result = JsonConvert.SerializeObject(account, DatabaseHelpers.CachedSerializer.Converters.ToArray());

            //#if DEBUG
            //            // Paulov: I was using this to test the final output and comparing instances
            //            {
            //                System.IO.File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "CharacterCreateResult.json"), result);
            //            }
            //            {
            //                var pmcDataResult = JsonConvert.SerializeObject(pmcData, DatabaseHelpers.CachedSerializer.Converters.ToArray());
            //                System.IO.File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "CharacterCreateResult_PMC.json"), pmcDataResult);
            //            }
            //            {
            //                var pmcData2Result = JsonConvert.SerializeObject(pmcData2, DatabaseHelpers.CachedSerializer.Converters.ToArray());
            //                System.IO.File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "CharacterCreateResult_PMC2.json"), pmcData2Result);
            //            }

            //#endif

            return new BSGSuccessBodyResult(result);

        }

        /// <summary>
        /// Called when the client searches for profiles in the game.
        /// </summary>
        /// <returns></returns>
        [Route("client/game/profile/search")]
        [HttpPost]
        public async Task<IActionResult> ProfileSearch()
        {
            /*
             * Expects json object with nickname defined as a string
             */

            var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);
            // If the request body is null or does not contain the "nickname" key, return an error response.
            if (requestBody == null || !requestBody.ContainsKey("nickname"))
            {
                return new BSGErrorBodyResult(402, "Request Body cannot be found or nickname is not provided!");
            }

            var sessionId = SessionId;

            var allProfiles = _saveProvider.GetProfiles();

            List<Dictionary<string, object>> chatMembers = new();
            // TODO: This needs refactoring. If we had a lot of profiles on this server then this could take a long period of time.
            foreach (var p in allProfiles)
            {
                if (p.Key == sessionId)
                    continue; // Skip the current profile

                var m = _accountService.GetUpdatableChatMember(p.Value, "PVE");
                if (m == null)
                    continue;

                var mInfo = m["Info"] as UpdatableChatMember.UpdatableChatMemberInfo;
                if (mInfo.Nickname.Contains(requestBody["nickname"].ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    chatMembers.Add(m);
                }
            }

            return new BSGSuccessBodyResult(chatMembers.ToArray());

        }

        [Route("client/game/profile/list")]
        [HttpPost]
        public IActionResult ProfileList(int? retry, bool? debug)
        {
            var gameMode = HttpContext.Session != null && HttpContext.Session.GetString("GameMode") != null ? HttpContext.Session.GetString("GameMode") : "pve";

            var sessionId = SessionId;
            var account = _saveProvider.LoadProfile(SessionId);
            if (account == null)
            {
#if DEBUG
                sessionId = _saveProvider.GetProfiles().Keys.First();
                // if we are running from Swagger and havent "logged in". just get this here
                account = _saveProvider.LoadProfile(sessionId);
#else
                Response.StatusCode = 500;
                return new BSGErrorBodyResult(500, "Profile has not been loaded!");
#endif
            }

            List<AccountProfileCharacter> list = new();
            var pmcProfile = _saveProvider.GetPmcProfile(account);
            if (pmcProfile != null)
                list.Add(pmcProfile);
            var scavProfile = _saveProvider.GetScavProfile(account);
            if (scavProfile != null)
                list.Add(scavProfile);

            return new BSGSuccessBodyResult(list);
        }

        [Route("client/game/profile/nickname/reserved")]
        [HttpPost]
        public IActionResult NicknameReserved()
        {
            var sessionId = SessionId;
#if DEBUG
            if (string.IsNullOrEmpty(sessionId))
                sessionId = _saveProvider.GetProfiles().Keys.First();
#endif
            var name = _saveProvider.GetProfiles()[sessionId].Username;

            return new BSGSuccessBodyResult(name);

        }

        [Route("client/game/profile/nickname/validate")]
        [HttpPost]
        public async Task<IActionResult> NicknameValidate()
        {
            var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);
            if (requestBody == null)
                return new BSGErrorBodyResult(402, "Request Body cannot be found!");

            if (!requestBody.ContainsKey("nickname"))
            {
                return new BSGErrorBodyResult(402, "nickname is not provided!");
            }

            if (requestBody["nickname"].ToString().Length < 3)
            {
                await HttpBodyConverters.CompressIntoResponseBodyBSG(null, Request, Response, 256, "The nickname is too short");
                return null;
            }
            //else if (saveProvider.NameExists(requestBody["nickname"].ToString()))
            //{
            //    await HttpBodyConverters.CompressIntoResponseBodyBSG(null, Request, Response, 255, "The nickname is already in use");
            //    return;
            //}

            JObject obj = new();
            obj.TryAdd("status", "ok");

            return new BSGSuccessBodyResult(obj);

        }

        [Route("client/profile/view")]
        [HttpPost]
        public async Task<IActionResult> ProfileView()
        {
            var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);
            if (requestBody == null)
                return new BSGErrorBodyResult(402, "Request Body cannot be found!");

            if (!requestBody.ContainsKey("accountId"))
            {
                return new BSGErrorBodyResult(402, "accountId is not provided!");
            }
            var accountAID = requestBody["accountId"].ToString();
            var otherAccount = _accountService.GetAccountByAID(accountAID);
            var pmcCharacter = _saveProvider.GetPmcProfile(otherAccount);
            var scavCharacter = _saveProvider.GetScavProfile(otherAccount);

            var favoriteItems = new JArray();
            var id = pmcCharacter.Id.ToString();
            var aid = accountAID;
            var info = JObject.FromObject(pmcCharacter.Info);
            var achievements = JObject.FromObject(pmcCharacter.AchievementsData);
            var customization = JObject.FromObject(pmcCharacter.Customization);
            var equipment = new JObject()
            {
                { "Id", _inventoryService.GetEquipmentId(pmcCharacter) },
                { "Items", JArray.FromObject(_inventoryService.GetInventoryItems(pmcCharacter), DatabaseHelpers.CachedSerializer) },
            };
            var pmcStats = JObject.FromObject(pmcCharacter.Stats);
            var scavStats = JObject.FromObject(pmcCharacter.Stats);
            var skills = JObject.FromObject(pmcCharacter.Skills);
            var hideout = JObject.FromObject(pmcCharacter.Hideout);
            var customizationStash = _inventoryService.GetHideoutCustomizationStashId(pmcCharacter);
            var hideoutAreaStashes = _inventoryService.GetHideoutAreaStashes(pmcCharacter);

            // Get the hideoutKeys
            var hideoutKeys = hideoutAreaStashes.Keys.Select(x => x.ToString()).ToList();
            hideoutKeys.Add(_inventoryService.GetHideoutCustomizationStashId(pmcCharacter));

            var itemsToReturn = new JArray();

            var profileView = new JObject()
            {
                { "favoriteItems", favoriteItems },
                { "id", id },
                { "aid", aid },
                { "info", info },
                { "achievements", achievements },
                { "customization", customization },
                { "equipment", equipment },
                { "pmcStats", pmcStats },
                //{ "scavStats", JObject.FromObject(scavCharacter.Stats) },
                { "scavStats", pmcStats },
                { "skills", skills },
                { "hideout", hideout },
                { "customizationStash", customizationStash },
                { "hideoutAreaStashes", JObject.FromObject(hideoutAreaStashes, DatabaseHelpers.CachedSerializer) },
                { "items", itemsToReturn }
            };

            return new BSGSuccessBodyResult(profileView);

        }

    }


    public static class ExtendForSeedText
    {
        private static readonly int[] int_0 = new int[23]
        {
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
            0, 0, 0, 0, 0, 0, 0, 10, 11, 12,
            13, 14, 15
        };

        public static byte[] FromHexString(this string hex)
        {
            byte[] array = new byte[hex.Length / 2];
            int num = 0;
            int num2 = 0;
            while (num2 < hex.Length)
            {
                array[num] = (byte)((int_0[char.ToUpper(hex[num2]) - 48] << 4) | int_0[char.ToUpper(hex[num2 + 1]) - 48]);
                num2 += 2;
                num++;
            }
            return array;
        }
    }
}
