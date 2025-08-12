using BSGHelperLibrary.ResponseModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Paulov.Tarkov.WebServer.DOTNET.Middleware;
using Paulov.TarkovModels;
using Paulov.TarkovServices.Providers.Interfaces;
using Paulov.TarkovServices.Services.Interfaces;

namespace Paulov.Tarkov.WebServer.DOTNET.Controllers
{
    /**
     * TODO: All of the functionality enclosed here needs to be refactored into FriendshipService
     */


    /// <summary>
    /// Provides functionality for managing friendships, including adding friends, sending and handling friend requests,
    /// and retrieving friend lists. This controller interacts with the social network features of user accounts.
    /// </summary>
    /// <remarks>The <see cref="FriendController"/> class exposes endpoints for managing friendships in a
    /// social network context.  It supports operations such as adding friends, sending friend requests, accepting or
    /// rejecting requests,  and retrieving lists of friends or pending requests. The controller relies on injected
    /// services for  persistence, friendship management, and WebSocket notifications.  This controller is designed to
    /// handle HTTP POST requests and is intended to be used in a web application  environment. It assumes the presence
    /// of a valid session ID for identifying the current user.</remarks>
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

            if (_websocketService != null)
            {
                // Notify the receiver about the new friend request
                var updatableChatMember = _friendshipService.CreateUpdatableChatMemberJObject(_saveProvider.LoadProfile(SessionId));
                if (updatableChatMember != null)
                {
                    _websocketService.SendNotificationToWebSocket(toId
                        , EFT.Communications.ENotificationType.FriendsListNewRequest
                        , new JObject()
                        {
                        { "profile", JObject.FromObject(updatableChatMember) }
                        }
                        );
                }
            }

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

        [Route("client/friend/request/accept-all")]
        [HttpPost]
        public async Task<IActionResult> FriendRequestAcceptAll()
        {
            var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);

            var account = _saveProvider.LoadProfile(SessionId);
            var accountProfileMode = _saveProvider.GetAccountProfileMode(account);

            foreach (var friendRequest in accountProfileMode.SocialNetwork.FriendRequestInbox)
            {
                _friendshipService.AcceptFriendRequest(friendRequest.FromId, friendRequest.ToId);
            }

            return new BSGSuccessBodyResult("OK");
        }


        [Route("client/friend/request/accept")]
        [HttpPost]
        public async Task<IActionResult> FriendRequestAccept()
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

                    // Notify the receiver about the added friend
                    _websocketService.SendNotificationToWebSocket(item.FromId
                        , EFT.Communications.ENotificationType.FriendsListAccept
                        , new JObject()
                        {
                        { "profile", JObject.FromObject(_friendshipService.CreateUpdatableChatMemberJObject(_saveProvider.LoadProfile(SessionId))) }
                        }
                        );
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

        [Route("client/friend/request/decline")]
        [HttpPost]
        public async Task<IActionResult> FriendRequestReject(int? retry, bool? debug)
        {
            var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);
            {
                var account = _saveProvider.LoadProfile(SessionId);
                var accountProfileMode = _saveProvider.GetAccountProfileMode(account);
                var myFriendRequestInboxIndex = accountProfileMode.SocialNetwork.FriendRequestInbox.FindIndex(x => x.FromId == requestBody["profileId"].ToString());
                if (myFriendRequestInboxIndex != -1)
                {
                    // remove from my account friend request inbox
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
                    // remove from other account friend request outbox
                    otherAccountMode.SocialNetwork.FriendRequestOutbox.RemoveAt(otherFriendRequestOutputIndex);
                }
                _saveProvider.SaveProfile(requestBody["profileId"].ToString(), otherAccount);
            }
            return new BSGSuccessBodyResult("OK");
        }

        [Route("client/friend/request/cancel")]
        [HttpPost]
        public async Task<IActionResult> FriendRequestCancel()
        {
            var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);
            {
                var account = _saveProvider.LoadProfile(SessionId);
                var accountProfileMode = _saveProvider.GetAccountProfileMode(account);
                var myFriendRequestInboxIndex = accountProfileMode.SocialNetwork.FriendRequestOutbox.FindIndex(x => x.ToId == requestBody["profileId"].ToString());
                if (myFriendRequestInboxIndex != -1)
                {
                    // remove from my account friend request inbox
                    accountProfileMode.SocialNetwork.FriendRequestOutbox.RemoveAt(myFriendRequestInboxIndex);
                }
                _saveProvider.SaveProfile(SessionId, account);
            }
            {
                var otherAccount = _saveProvider.LoadProfile(requestBody["profileId"].ToString());
                var otherAccountMode = _saveProvider.GetAccountProfileMode(otherAccount);
                var otherFriendRequestOutputIndex = otherAccountMode.SocialNetwork.FriendRequestInbox.FindIndex(x => x.ToId == requestBody["profileId"].ToString());
                if (otherFriendRequestOutputIndex != -1)
                {
                    // remove from other account friend request outbox
                    otherAccountMode.SocialNetwork.FriendRequestInbox.RemoveAt(otherFriendRequestOutputIndex);
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

            JArray ignoreArray = new JArray();
            foreach (var ignoreId in accountProfileMode.SocialNetwork.Ignore)
            {
                ignoreArray.Add(_friendshipService.CreateUpdatableChatMemberJObject(_saveProvider.LoadProfile(ignoreId)));
            }

            JObject packet = new();
            packet.Add("Friends", friendsArray);
            packet.Add("Ignore", ignoreArray);
            packet.Add("InIgnoreList", new JArray());
            return await Task.FromResult(new BSGSuccessBodyResult(packet));
        }

    }
}
