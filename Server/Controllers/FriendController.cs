using BSGHelperLibrary.ResponseModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Paulov.Tarkov.WebServer.DOTNET.Middleware;
using Paulov.TarkovModels;
using Paulov.TarkovServices.Providers.Interfaces;
using Paulov.TarkovServices.Services.Interfaces;

namespace Paulov.Tarkov.WebServer.DOTNET.Controllers
{
    public class FriendController : Controller
    {
        private ISaveProvider _saveProvider;
        private IFriendshipService _friendshipService;
        private IWebSocketService _websocketService;

        public FriendController(ISaveProvider saveProvider, IFriendshipService friendshipService, IWebSocketService webSocketService)
        {
            _saveProvider = saveProvider;
            _friendshipService = friendshipService;
            _websocketService = webSocketService;
        }

        private string SessionId
        {
            get
            {
                return HttpSessionHelpers.GetSessionId(Request, HttpContext);
            }
        }

        [HttpPost]
        public IActionResult AddFriend(string sessionId, string friendId)
        {
            if (string.IsNullOrEmpty(sessionId) || string.IsNullOrEmpty(friendId))
            {
                return BadRequest("Session ID and Friend ID cannot be empty.");
            }
            var account = _saveProvider.LoadProfile(sessionId);
            if (account == null)
            {
                return NotFound("Account not found.");
            }
            AccountProfileMode accountProfileMode = _saveProvider.GetAccountProfileMode(account);
            accountProfileMode.SocialNetwork.Friends.Add(friendId);
            if (accountProfileMode.SocialNetwork.Friends.Contains(friendId))
            {
                return Conflict("Friend already added.");
            }
            accountProfileMode.SocialNetwork.Friends.Add(friendId);
            _saveProvider.SaveProfile(sessionId, account);
            return Ok("Friend added successfully.");
        }

        [Route("client/friend/request/send")]
        [HttpPost]
        public async Task<IActionResult> SendFriendRequest()
        {
            var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);

            var toId = requestBody.GetValueOrDefault("to")?.ToString();
            var requestId = _friendshipService.SendFriendRequest(
                SessionId,
                toId
            );

            if (requestId == null)
            {
                return new BSGErrorBodyResult(500, "");
            }

            //_websocketService.GetWebSocket(SessionId)?
            //    .SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes($"{{\"type\":\"FriendRequestSent\",\"eventId\":\"friendListNewRequest\"}}")), System.Net.WebSockets.WebSocketMessageType.Text, true, CancellationToken.None);

            //_websocketService.GetWebSocket(toId)?
            //  .SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes($"{{\"type\":\"friendListNewRequest\",\"eventId\":\"friendListNewRequest\"}}")), System.Net.WebSockets.WebSocketMessageType.Text, true, CancellationToken.None);

            JObject result = new JObject()
            {
                { "requestId", requestId.ToString() },
                { "status", "None" }
            };
            return new BSGSuccessBodyResult(result);

        }

        [Route("client/friend/request/list/inbox")]
        [HttpPost]
        public async Task<IActionResult> FriendRequestInbox(int? retry, bool? debug)
        {
            var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);

            var account = _saveProvider.LoadProfile(SessionId);
            var accountProfileMode = _saveProvider.GetAccountProfileMode(account);

            JArray friendRequests = new JArray();
            foreach (var request in accountProfileMode.SocialNetwork.FriendRequestInbox)
            {
                var fromAccount = _saveProvider.LoadProfile(request.FromId);

                var friendRequest = new JObject
                {
                    ["_id"] = request.FriendRequestId.ToString(),
                    ["from"] = request.FromId,
                    ["to"] = request.ToId,
                    ["date"] = 0,
                    ["profile"] = JObject.FromObject(_friendshipService.CreateUpdatableChatMemberJObject(fromAccount, null))
                };
                friendRequests.Add(friendRequest);
            }

            return new BSGSuccessBodyResult(friendRequests);
        }

        [Route("client/friend/request/list/outbox")]
        [HttpPost]
        public async Task<IActionResult> FriendRequestOutbox(int? retry, bool? debug)
        {
            var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);

            var account = _saveProvider.LoadProfile(SessionId);
            var accountProfileMode = _saveProvider.GetAccountProfileMode(account);

            JArray friendRequests = new JArray();
            foreach (var request in accountProfileMode.SocialNetwork.FriendRequestOutbox)
            {
                var toAccount = _saveProvider.LoadProfile(request.ToId);

                var friendRequest = new JObject
                {
                    ["_id"] = request.FriendRequestId.ToString(),
                    ["from"] = request.FromId,
                    ["to"] = request.ToId,
                    ["date"] = 0,
                    ["profile"] = JObject.FromObject(_friendshipService.CreateUpdatableChatMemberJObject(toAccount, null))
                };
                friendRequests.Add(friendRequest);
            }

            return new BSGSuccessBodyResult(friendRequests);
        }


        [Route("client/friend/request/accept")]
        [HttpPost]
        public async Task<IActionResult> FriendRequestAccept(int? retry, bool? debug)
        {
            var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);

            {
                var account = _saveProvider.LoadProfile(SessionId);
                var accountProfileMode = _saveProvider.GetAccountProfileMode(account);

                var myFriendRequestInboxIndex = accountProfileMode.SocialNetwork.FriendRequestInbox.FindIndex(x => x.FromId == requestBody["profileId"].ToString());
                if (myFriendRequestInboxIndex != -1)
                {
                    // add to my account friend list
                    var item = accountProfileMode.SocialNetwork.FriendRequestInbox[myFriendRequestInboxIndex];
                    accountProfileMode.SocialNetwork.Friends.Add(item.FromId);
                    accountProfileMode.SocialNetwork.FriendRequestInbox.RemoveAt(myFriendRequestInboxIndex);
                }

                _saveProvider.SaveProfile(SessionId, account);

            }

            {
                var otherAccount = _saveProvider.LoadProfile(requestBody["profileId"].ToString());
                var otherAccountMode = _saveProvider.GetAccountProfileMode(otherAccount);

                var otherFriendRequestOutputIndex = otherAccountMode.SocialNetwork.FriendRequestOutbox.FindIndex(x => x.FromId == requestBody["profileId"].ToString());
                if (otherFriendRequestOutputIndex != -1)
                {
                    // add to other account friend list
                    var item = otherAccountMode.SocialNetwork.FriendRequestOutbox[otherFriendRequestOutputIndex];
                    otherAccountMode.SocialNetwork.Friends.Add(item.ToId);
                    otherAccountMode.SocialNetwork.FriendRequestOutbox.RemoveAt(otherFriendRequestOutputIndex);
                }

                _saveProvider.SaveProfile(requestBody["profileId"].ToString(), otherAccount);
            }

            return new BSGSuccessBodyResult("OK");
        }


        [Route("client/friend/list")]
        [HttpPost]
        public async Task<IActionResult> FriendList(int? retry, bool? debug)
        {
            var account = _saveProvider.LoadProfile(SessionId);
            var accountProfileMode = _saveProvider.GetAccountProfileMode(account);

            JArray friendsArray = new JArray();
            foreach (var fId in accountProfileMode.SocialNetwork.Friends)
            {
                friendsArray.Add(_friendshipService.CreateUpdatableChatMemberJObject(_saveProvider.LoadProfile(fId)));
            }

            JObject packet = new();
            packet.Add("Friends", friendsArray);
            packet.Add("Ignore", new JArray());
            packet.Add("InIgnoreList", new JArray());
            return await Task.FromResult(new BSGSuccessBodyResult(packet));
        }

    }
}
