using BSGHelperLibrary.ResponseModels;
using ChatShared;
using EFT;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Paulov.Tarkov.WebServer.DOTNET.Middleware;
using Paulov.Tarkov.WebServer.DOTNET.Services;
using Paulov.TarkovModels;
using Paulov.TarkovServices;
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
            var profile = saveProvider.LoadProfile(SessionId);
            if (profile == null)
            {
#if DEBUG
                sessionId = saveProvider.GetProfiles().Any() ? saveProvider.GetProfiles().Keys.First() : MongoID.Generate(false);
                // if we are running from Swagger and havent "logged in". just get this here
                profile = saveProvider.LoadProfile(sessionId);

                if (profile == null)
                {
                    sessionId = saveProvider.CreateAccount(new Dictionary<string, object>() { { "username", "Swagger" }, { "password", "Swagger" }, { "edition", "Edge Of Darkness" } });
                    profile = saveProvider.LoadProfile(sessionId);
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

            GlobalsService.Instance.LoadGlobalsIntoComfortSingleton();

            if (!DatabaseProvider.TryLoadDatabaseFile("templates/profiles.json", out JObject profileTemplates))
            {
                Response.StatusCode = 500;
                return new BSGErrorBodyResult(500, "");
            }

            if (!DatabaseProvider.TryLoadDatabaseFile("templates/customization.json", out JObject customizationTemplates))
            {
                Response.StatusCode = 500;
                return new BSGErrorBodyResult(500, "");
            }

            var blankStat = new GClass2020()
            {
                CarriedQuestItems = new List<MongoID>(),
                DamageHistory = new GClass2016() { LethalDamagePart = EBodyPart.Head, BodyParts = new Dictionary<EBodyPart, GClass2015>() },
                DroppedItems = new List<GClass1985>(),
                ExperienceBonusMult = 0,
                FoundInRaidItems = new List<GClass1986>(),
                LastPlayerState = null,
                SessionCounters = new GClass2019(),
                OverallCounters = new GClass2019(),
                SessionExperienceMult = 0,
                SurvivorClass = ProfileStats.ESurvivorClass.Unknown,
                TotalInGameTime = 0,
                TotalSessionExperience = 0,
                Victims = new List<GClass2001>()
            };
            var blankStatGroup = new GClass2021()
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
            var pmcData = template.ToObject<AccountProfileCharacter>(DatabaseProvider.CachedSerializer);
            if (pmcData == null)
            {
                Response.StatusCode = 500;
                return new BSGErrorBodyResult(500, "");
            }

            pmcData.Info.MemberCategory = EMemberCategory.Default;
            pmcData.Info.SelectedMemberCategory = EMemberCategory.Default;

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
            var scavData = scavTemplate.ToObject<AccountProfileCharacter>(DatabaseProvider.CachedSerializer);
            scavData.Id = MongoID.Generate();
            pmcData.PetId = scavData.Id;

            // Assign the profiles -----------------------------------------------------------------------------------
            saveProvider.GetAccountProfileMode(sessionId).Characters.PMC = pmcData;
            saveProvider.GetAccountProfileMode(sessionId).Characters.Scav = scavData;

            saveProvider.CleanIdsOfInventory(profile);
            saveProvider.SaveProfile(sessionId, profile);

            requestBody = null;

            return new BSGSuccessBodyResult(JsonConvert.SerializeObject(profile));

        }

        [Route("client/game/profile/search")]
        [HttpPost]
        public async Task<IActionResult> ProfileSearch()
        {
            var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);

            var profile = saveProvider.LoadProfile(SessionId);
            if (profile == null)
            {
                Response.StatusCode = 500;
                return new NotFoundResult();
            }

            var allProfiles = saveProvider
                .GetProfiles();

            List<Dictionary<string, object>> chatMembers = new();
            foreach (var p in allProfiles)
            {
                var pmc = saveProvider.GetPmcProfile(p.Key);
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
            var profile = saveProvider.LoadProfile(SessionId);
            if (profile == null)
            {
#if DEBUG
                sessionId = saveProvider.GetProfiles().Keys.First();
                // if we are running from Swagger and havent "logged in". just get this here
                profile = saveProvider.LoadProfile(sessionId);
#else
                Response.StatusCode = 500;
                return new BSGErrorBodyResult(500, "Profile has not been loaded!");
#endif
            }

            List<AccountProfileCharacter> list = new();
            var pmcProfile = saveProvider.GetPmcProfile(sessionId);
            if (pmcProfile != null)
                list.Add(pmcProfile);
            var scavProfile = saveProvider.GetScavProfile(sessionId);
            if (scavProfile != null)
                list.Add(scavProfile);

            return new BSGSuccessBodyResult(list);
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
