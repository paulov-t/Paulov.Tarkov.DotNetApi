using ChatShared;
using EFT;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Paulov.Tarkov.WebServer.DOTNET.Middleware;
using Paulov.Tarkov.WebServer.DOTNET.Models;
using Paulov.Tarkov.WebServer.DOTNET.Providers;
using Paulov.Tarkov.WebServer.DOTNET.ResponseModels;
using Paulov.Tarkov.WebServer.DOTNET.Services;

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
                sessionId = saveProvider.GetProfiles().Keys.First();
                // if we are running from Swagger and havent "logged in". just get this here
                profile = saveProvider.LoadProfile(sessionId);
#else
                Response.StatusCode = 500;
                return;
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

            var template = profileTemplates[(string)profile.Edition][requestBody["side"].ToString().ToLower()]["character"];
            template["Customization"]["Head"] = requestBody["headId"].ToString();
            template["_id"] = sessionId;
            template["aid"] = new Random().Next(100000, 500000);
            template["savage"] = null;
            template["Info"]["Nickname"] = requestBody["nickname"].ToString();
            template["Info"]["LowerNickname"] = requestBody["nickname"].ToString().ToLower();
            template["Info"]["RegistrationDate"] = new Random().Next(100000, 500000);// (long)Math.Floor((decimal)DateTime.Now.Ticks / 1000);
            template["Info"]["Voice"] = customizationTemplates[requestBody["voiceId"].ToString()]["_name"];
            template["Stats"] = JToken.FromObject(new GClass2021()
            {
                Eft = new GClass2020()
                {

                }
                ,
                Arena = new GClass2020()
                {

                }
            });
            template["WishList"] = JToken.FromObject(new Dictionary<MongoID, byte>());
            // Get Template Profile
            var pmcData = template.ToObject<AccountProfileCharacter>();
            if (pmcData == null)
            {
                Response.StatusCode = 500;
                return new BSGErrorBodyResult(500, "");
            }

            profile.CurrentMode = gameMode;
            saveProvider.GetAccountProfileMode(sessionId).Characters.PMC = pmcData;

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
    }
}
