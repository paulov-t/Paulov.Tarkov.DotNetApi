using ChatShared;
using EFT;
using Newtonsoft.Json.Linq;
using Paulov.TarkovModels;
using Paulov.TarkovServices.Providers.Interfaces;
using Paulov.TarkovServices.Services.Interfaces;

namespace Paulov.TarkovServices.Services
{
    public sealed class FriendshipService : IFriendshipService
    {
        private ISaveProvider _saveProvider;
        private IWebSocketService _webSocketService;

        public FriendshipService(ISaveProvider saveProvider, IWebSocketService webSocketService)
        {
            _saveProvider = saveProvider;
            _webSocketService = webSocketService;
        }

        public MongoID? SendFriendRequest(string fromId, string toId)
        {
            var accountFrom = _saveProvider.LoadProfile(fromId);
            if (accountFrom == null)
                return null;

            var mode = accountFrom.CurrentMode;

            var accountTo = _saveProvider.LoadProfile(toId);
            if (accountTo == null)
                return null;

            AccountProfileMode accountProfileModeFrom = null;
            AccountProfileMode accountProfileModeTo = null;

            switch (mode.ToUpperInvariant())
            {
                case "REGULAR":
                    accountProfileModeFrom = accountFrom.Modes.Regular;
                    accountProfileModeTo = accountTo.Modes.Regular;
                    break;
                case "PVE":
                    accountProfileModeFrom = accountFrom.Modes.PVE;
                    accountProfileModeTo = accountTo.Modes.PVE;
                    break;
                case "ARENA":
                    accountProfileModeFrom = accountFrom.Modes.Arena;
                    accountProfileModeTo = accountTo.Modes.Arena;
                    break;
                default:
                    return null; // Invalid mode
            }

            if (accountProfileModeFrom == null)
                return null;

            if (accountProfileModeTo == null)
                return null;

            var requestId = MongoID.Generate(true);
            if (accountProfileModeFrom.SocialNetwork.FriendRequestOutbox.Any(x => x.ToId == toId && x.FromId == fromId) ||
                accountProfileModeTo.SocialNetwork.FriendRequestInbox.Any(x => x.ToId == fromId && x.FromId == toId))
            {
                return null; // Friend request already exists
            }
            accountProfileModeFrom.SocialNetwork.FriendRequestOutbox.Add(new FriendRequest() { FriendRequestId = requestId, FromId = fromId, ToId = toId, Date = DateTime.Now });
            accountProfileModeTo.SocialNetwork.FriendRequestInbox.Add(new FriendRequest() { FriendRequestId = requestId, FromId = fromId, ToId = toId, Date = DateTime.Now });

            _saveProvider.SaveProfile(accountFrom.AccountId, accountFrom);
            _saveProvider.SaveProfile(accountTo.AccountId, accountTo);
            return requestId;
        }

        public void DeclineFriendRequest(string fromId, string toId, MongoID requestId)
        {
            var accountFrom = _saveProvider.LoadProfile(fromId);
            if (accountFrom == null)
                return;
            var mode = accountFrom.CurrentMode;
            var accountTo = _saveProvider.LoadProfile(toId);
            if (accountTo == null)
                return;
            AccountProfileMode accountProfileModeFrom = null;
            AccountProfileMode accountProfileModeTo = null;
            switch (mode.ToUpperInvariant())
            {
                case "REGULAR":
                    accountProfileModeFrom = accountFrom.Modes.Regular;
                    accountProfileModeTo = accountTo.Modes.Regular;
                    break;
                case "PVE":
                    accountProfileModeFrom = accountFrom.Modes.PVE;
                    accountProfileModeTo = accountTo.Modes.PVE;
                    break;
                case "ARENA":
                    accountProfileModeFrom = accountFrom.Modes.Arena;
                    accountProfileModeTo = accountTo.Modes.Arena;
                    break;
                default:
                    return; // Invalid mode
            }
            if (accountProfileModeFrom == null || accountProfileModeTo == null)
                return;
            var requestOutbox = accountProfileModeFrom.SocialNetwork.FriendRequestOutbox.FirstOrDefault(x => x.FriendRequestId == requestId && x.ToId == toId && x.FromId == fromId);
            if (requestOutbox != null)
            {
                accountProfileModeFrom.SocialNetwork.FriendRequestOutbox.Remove(requestOutbox);
            }
            var requestInbox = accountProfileModeTo.SocialNetwork.FriendRequestInbox.FirstOrDefault(x => x.FriendRequestId == requestId && x.ToId == fromId && x.FromId == toId);
            if (requestInbox != null)
            {
                accountProfileModeTo.SocialNetwork.FriendRequestInbox.Remove(requestInbox);
            }
            _saveProvider.SaveProfile(accountFrom.AccountId, accountFrom);
            _saveProvider.SaveProfile(accountTo.AccountId, accountTo);
        }

        public void AcceptFriendRequest(string fromId, string toId)
        {
            {
                var account = _saveProvider.LoadProfile(fromId);
                var accountProfileMode = _saveProvider.GetAccountProfileMode(account);

                var myFriendRequestInboxIndex = accountProfileMode.SocialNetwork.FriendRequestInbox.FindIndex(x => x.FromId == fromId);
                if (myFriendRequestInboxIndex != -1)
                {
                    // add to my account friend list
                    var item = accountProfileMode.SocialNetwork.FriendRequestInbox[myFriendRequestInboxIndex];
                    accountProfileMode.SocialNetwork.Friends.Add(item.FromId);
                    accountProfileMode.SocialNetwork.FriendRequestInbox.RemoveAt(myFriendRequestInboxIndex);
                }

                _saveProvider.SaveProfile(fromId, account);

            }

            {
                var otherAccount = _saveProvider.LoadProfile(toId);
                var otherAccountMode = _saveProvider.GetAccountProfileMode(otherAccount);

                var otherFriendRequestOutputIndex = otherAccountMode.SocialNetwork.FriendRequestOutbox.FindIndex(x => x.FromId == fromId);
                if (otherFriendRequestOutputIndex != -1)
                {
                    // add to other account friend list
                    var item = otherAccountMode.SocialNetwork.FriendRequestOutbox[otherFriendRequestOutputIndex];
                    otherAccountMode.SocialNetwork.Friends.Add(item.ToId);
                    otherAccountMode.SocialNetwork.FriendRequestOutbox.RemoveAt(otherFriendRequestOutputIndex);
                }

                _saveProvider.SaveProfile(toId, otherAccount);
            }
        }

        public JObject CreateUpdatableChatMemberJObject(Account account, string gameMode = null)
        {

            AccountProfileMode profile = null;
            if (string.IsNullOrEmpty(gameMode))
                profile = _saveProvider.GetAccountProfileMode(account);
            else
            {
                throw new NotImplementedException();
                //profile = account.Modes
            }

            var pmc = _saveProvider.GetPmcProfile(account);
            var jobj = new JObject();
            jobj.Add("AccountId", pmc.AccountId);
            jobj.Add("_id", pmc.Id.ToString());
            jobj.Add("aid", pmc.AccountId);
            jobj.Add("Info", JObject.FromObject(new UpdatableChatMember.UpdatableChatMemberInfo()
            {
                Nickname = pmc.Info.Nickname,
                Side = pmc.Info.Side == EPlayerSide.Usec ? EChatMemberSide.Usec : pmc.Info.Side == EPlayerSide.Bear ? EChatMemberSide.Bear : EChatMemberSide.Trader,
                Level = pmc.Info.Level,
                MemberCategory = pmc.Info.MemberCategory,
                SelectedMemberCategory = pmc.Info.SelectedMemberCategory,
                Banned = false
            }));

            return jobj;

        }
    }

} // End of namespace
