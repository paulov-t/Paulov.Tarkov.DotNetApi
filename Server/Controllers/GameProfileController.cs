using BSGHelperLibrary.ResponseModels;
using ChatShared;
using EFT;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Paulov.Tarkov.WebServer.DOTNET.Middleware;
using Paulov.TarkovModels;
using Paulov.TarkovServices.Providers.Interfaces;
using Paulov.TarkovServices.Providers.SaveProviders;
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
        private JsonFileSaveProvider _saveProvider;
        private IGlobalsService _globalsService;

        public GameProfileController(ISaveProvider saveProvider, IGlobalsService globalsService)
        {
            _saveProvider = saveProvider as JsonFileSaveProvider;
            _globalsService = globalsService;
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
            var profile = _saveProvider.LoadProfile(sessionId);
            var mode = _saveProvider.GetAccountProfileMode(sessionId);

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
            var profile = _saveProvider.LoadProfile(SessionId);
            if (profile == null)
            {
#if DEBUG
                sessionId = _saveProvider.GetProfiles().Any() ? _saveProvider.GetProfiles().Keys.First() : MongoID.Generate(false);
                // if we are running from Swagger and havent "logged in". just get this here
                profile = _saveProvider.LoadProfile(sessionId);

                if (profile == null)
                {
                    sessionId = _saveProvider.CreateAccount(new Dictionary<string, object>() { { "username", "Swagger" }, { "password", "Swagger" }, { "edition", "Edge Of Darkness" } });
                    profile = _saveProvider.LoadProfile(sessionId);
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

            if (!DatabaseService.TryLoadDatabaseFile("templates/profiles.json", out JObject profileTemplates))
            {
                Response.StatusCode = 500;
                return new BSGErrorBodyResult(500, "");
            }

            if (!DatabaseService.TryLoadDatabaseFile("templates/customization.json", out JObject customizationTemplates))
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

            var template = profileTemplates[(string)profile.Edition][requestBody["side"].ToString().ToLower()]["character"];
            template["Customization"]["Head"] = requestBody["headId"].ToString();
            template["_id"] = sessionId;
            template["aid"] = new Random().Next(100000, 500000);
            template["savage"] = null;
            template["Info"]["Nickname"] = requestBody["nickname"].ToString();
            template["Info"]["LowerNickname"] = requestBody["nickname"].ToString().ToLower();
            template["Info"]["RegistrationDate"] = new Random().Next(100000, 500000);
            template["Info"]["Voice"] = customizationTemplates[requestBody["voiceId"].ToString()]["_name"];
            template["Stats"] = JToken.FromObject(blankStatGroup);
            template["WishList"] = JToken.FromObject(new Dictionary<MongoID, byte>());
            template["Hideout"]["Seed"] = "";
            var hideoutCheck = template["Hideout"];
            // Get Template Profile
            var pmcData = template.ToObject<AccountProfileCharacter>(DatabaseService.CachedSerializer);
            if (pmcData == null)
            {
                Response.StatusCode = 500;
                return new BSGErrorBodyResult(500, "");
            }

            pmcData.Info.MemberCategory = EMemberCategory.Default;
            pmcData.Info.SelectedMemberCategory = EMemberCategory.Default;

            if (gameMode != null)
                profile.CurrentMode = gameMode;

            // Create scav -------------------------------------------------------------------------------------------
            var scavTemplateResource = FMT.FileTools.EmbeddedResourceHelper.GetEmbeddedResourceByName("scav.json");
            using var msScavTemplate = new MemoryStream();
            scavTemplateResource.CopyTo(msScavTemplate);
            var bytesOfScavTemplateResource = msScavTemplate.ToArray();
            var scavTemplateText = Encoding.UTF8.GetString(bytesOfScavTemplateResource);
            var scavTemplate = JObject.Parse(scavTemplateText)["scav"];
            scavTemplate["Inventory"] = template["Inventory"].DeepClone();
            scavTemplate["Stats"] = JToken.FromObject(blankStatGroup);
            var scavData = scavTemplate.ToObject<AccountProfileCharacter>(DatabaseService.CachedSerializer);
            scavData.Id = MongoID.Generate();
            pmcData.PetId = scavData.Id;

            // Assign the profiles -----------------------------------------------------------------------------------
            _saveProvider.GetAccountProfileMode(sessionId).Characters.PMC = pmcData;
            _saveProvider.GetAccountProfileMode(sessionId).Characters.Scav = scavData;

            _saveProvider.CleanIdsOfInventory(profile);
            _saveProvider.SaveProfile(sessionId, profile);

            requestBody = null;

            return new BSGSuccessBodyResult(JsonConvert.SerializeObject(profile));

        }

        [Route("client/game/profile/search")]
        [HttpPost]
        public async Task<IActionResult> ProfileSearch()
        {
            var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);

            var profile = _saveProvider.LoadProfile(SessionId);
            if (profile == null)
            {
                Response.StatusCode = 500;
                return new NotFoundResult();
            }

            var allProfiles = _saveProvider.GetProfiles();

            List<Dictionary<string, object>> chatMembers = new();
            foreach (var p in allProfiles)
            {
                var pmc = _saveProvider.GetPmcProfile(p.Key);
                var info = new UpdatableChatMember.UpdatableChatMemberInfo();
                info.Nickname = pmc.Info.Nickname;// pmc["Info"]["Nickname"].ToString();
                info.Side = EFT.EChatMemberSide.Usec;
                info.Banned = false;
                info.Ignored = false;
                info.Level = 1;
                info.MemberCategory = EMemberCategory.Default;
                info.SelectedMemberCategory = EMemberCategory.Default;

                var member = new Dictionary<string, object>();
                member.Add("Id", p.Key);
                member.Add("Info", info);
                chatMembers.Add(member);
            }
            //UpdatableChatMember[] chatMembers = saveProvider
            //    .GetProfiles()
            //    .Values
            //    .SelectMany(profile =>
            //    {
            //        new UpdatableChatMember(profile.AccountId.ToString())
            //        {
            //            AccountId = profile.AccountId.ToString()
            //        };
            //    })
            //    .ToArray();

            return new BSGSuccessBodyResult(chatMembers.ToArray());

        }

        [Route("client/game/profile/list")]
        [HttpPost]
        public IActionResult ProfileList(int? retry, bool? debug)
        {
            var gameMode = HttpContext.Session != null && HttpContext.Session.GetString("GameMode") != null ? HttpContext.Session.GetString("GameMode") : "pve";

            var sessionId = SessionId;
            var profile = _saveProvider.LoadProfile(SessionId);
            if (profile == null)
            {
#if DEBUG
                sessionId = _saveProvider.GetProfiles().Keys.First();
                // if we are running from Swagger and havent "logged in". just get this here
                profile = _saveProvider.LoadProfile(sessionId);
#else
                Response.StatusCode = 500;
                return new BSGErrorBodyResult(500, "Profile has not been loaded!");
#endif
            }

            List<AccountProfileCharacter> list = new();
            var pmcProfile = _saveProvider.GetPmcProfile(sessionId);
            if (pmcProfile != null)
                list.Add(pmcProfile);
            var scavProfile = _saveProvider.GetScavProfile(sessionId);
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
