using ChatShared;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Paulov.Tarkov.WebServer.DOTNET.Middleware;
using Paulov.Tarkov.WebServer.DOTNET.Models;
using Paulov.Tarkov.WebServer.DOTNET.Providers;
using Paulov.Tarkov.WebServer.DOTNET.ResponseModels;

namespace Paulov.Tarkov.Web.Api.Controllers
{
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

        [Route("client/game/profile/create")]
        [HttpPost]
        public async void ProfileCreate()
        {
            var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);

            var gameMode = HttpContext.Session.GetString("GameMode");

            var profile = saveProvider.LoadProfile(SessionId);
            if (profile == null)
            {
                Response.StatusCode = 500;
                return;
            }

            if (!DatabaseProvider.TryLoadDatabaseFile("templates/profiles.json", out JObject profileTemplates))
            {
                Response.StatusCode = 500;
                return;
            }

            if (!DatabaseProvider.TryLoadDatabaseFile("templates/customization.json", out JObject customizationTemplates))
            {
                Response.StatusCode = 500;
                return;
            }

            var template = profileTemplates[(string)profile.Edition][requestBody["side"].ToString().ToLower()]["character"];
            template["Customization"]["Head"] = requestBody["headId"].ToString();
            template["_id"] = SessionId;
            template["aid"] = new Random().Next(100000, 500000);
            template["savage"] = null;
            template["Info"]["Nickname"] = requestBody["nickname"].ToString();
            template["Info"]["LowerNickname"] = requestBody["nickname"].ToString().ToLower();
            template["Info"]["RegistrationDate"] = new Random().Next(100000, 500000);// (long)Math.Floor((decimal)DateTime.Now.Ticks / 1000);
            template["Info"]["Voice"] = customizationTemplates[requestBody["voiceId"].ToString()]["_name"];
            // Get Template Profile
            var pmcData = template.ToObject<AccountProfileCharacter2>();
            if (pmcData == null)
            {
                Response.StatusCode = 500;
                return;
            }

            if (!DatabaseProvider.TryLoadCustomization(out var customization))
            {
                Response.StatusCode = 500;
                return;
            }

            pmcData.Id = SessionId;
            if (requestBody == null)
            {
                Response.StatusCode = 412; // pre condition
                return;
            }

            if (!requestBody.ContainsKey("nickname"))
            {
                Response.StatusCode = 412; // pre condition
                return;
            }

            var pmcDataInfo = pmcData.Info;
            pmcDataInfo.Nickname = requestBody["nickname"].ToString();
            pmcDataInfo.Nickname = requestBody["nickname"].ToString().ToLower();
            //pmcDataInfo.RegistrationDate = (int)Math.Round((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds);
            //pmcDataInfo.Voice = ((JToken)customization[requestBody["voiceId"].ToString()])["_name"];

            //var pmcCustomizationInfo = ((JToken)pmcData["Customization"]).ToObject<Dictionary<string, dynamic>>();
            //pmcCustomizationInfo["Head"] = requestBody["headId"].ToString();
            //pmcData["Customization"] = pmcCustomizationInfo;

            //profile.Characters["pmc"] = JObject.Parse(pmcData.ToJson());
            //profile.Characters["scav"] = null;
            //profile.Info["wipe"] = false;

            saveProvider.CleanIdsOfInventory(profile);
            saveProvider.SaveProfile(SessionId, profile);

            await HttpBodyConverters.CompressIntoResponseBodyBSG(JsonConvert.SerializeObject(profile), Request, Response);
            requestBody = null;

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
