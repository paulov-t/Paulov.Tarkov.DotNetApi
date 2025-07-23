using BSGHelperLibrary.ResponseModels;
using EFT;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Paulov.Tarkov.WebServer.DOTNET.Middleware;
using Paulov.TarkovModels.NotificationModels;
using Paulov.TarkovServices.Providers.Interfaces;
using Paulov.TarkovServices.Services.Interfaces;

namespace Paulov.Tarkov.WebServer.DOTNET.Controllers
{
    public class RaidController : Controller
    {
        private ISaveProvider _saveProvider;
        private IAccountService _accountService;
        private IWebSocketService _webSocketService;
        private IMatchingService _matchingService;

        public RaidController
            (
            ISaveProvider saveProvider
            , IAccountService accountService
            , IWebSocketService webSocketService
            , IMatchingService matchingService
            )
        {
            _saveProvider = saveProvider ?? throw new ArgumentNullException(nameof(saveProvider));
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
            _webSocketService = webSocketService ?? throw new ArgumentNullException(nameof(webSocketService));
            _matchingService = matchingService ?? throw new ArgumentNullException(nameof(matchingService));
        }

        private string SessionId
        {
            get
            {
                return HttpSessionHelpers.GetSessionId(Request, HttpContext);
            }
        }

        /// <summary>
        /// Updates the raid configuration for the current session.
        /// </summary>
        /// <remarks>This method processes a decompressed request body containing raid configuration data,
        /// updates the account profile with the new configuration, and saves the updated profile. Does not expect a response.</remarks>
        /// <returns>An <see cref="IActionResult"/> indicating the success of the operation.</returns>
        [Route("client/raid/configuration")]
        [HttpPost]
        public async Task<IActionResult> RaidConfiguration()
        {
            var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);

            var account = _saveProvider.LoadProfile(SessionId);
            var accountProfile = _saveProvider.GetAccountProfileMode(account);
            accountProfile.RaidConfiguration = JObject.FromObject(requestBody).ToObject<RaidSettings>();
            _saveProvider.SaveProfile(SessionId, account);

            var group = _matchingService.GetMatchingGroupBySessionId(SessionId);

            // If the player is not in a group, we don't need to send any notifications
            if (group == null)
                return new BSGSuccessBodyResult(new { });

            // If the player is not the squad leader, we don't need to send any notifications
            if (SessionId != group.SquadLeaderId)
                return new BSGSuccessBodyResult(new { });

            // If the player is the squad leader, we need to send the updated raid configuration to all squad members
            var groupMember = _accountService.GetMatchingGroupMemberSquadPlayer(_saveProvider.LoadProfile(SessionId), SessionId == group.SquadLeaderId, true, null);
            var squadMembers = group.Members.Select(memberId => _accountService.GetMatchingGroupMemberSquadPlayer(_saveProvider.LoadProfile(memberId), memberId == group.SquadLeaderId, true, null)).ToList();
            foreach (var member in squadMembers)
            {
                if (member.Id != SessionId)
                {
                    await _webSocketService.SendNotificationToWebSocket(member.Id, new GroupMatchRaidSettingsModel(_saveProvider.GetAccountProfileMode(account).RaidConfiguration), null);
                    await Task.Delay(1500); // To prevent flooding the WebSocket with messages
                }
            }

            return new BSGSuccessBodyResult(new { });
        }
    }
}
