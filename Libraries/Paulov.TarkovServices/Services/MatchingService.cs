using EFT;
using Paulov.TarkovModels.GroupingModels;
using Paulov.TarkovModels.ServerModels;
using Paulov.TarkovServices.Services.Interfaces;

namespace Paulov.TarkovServices.Services
{
    public class MatchingService : IMatchingService
    {
        private IAccountService _accountService;

        private object LockingObject = new();
        public List<MatchingGroup> MatchingGroups { get; } = new();
        public List<ServerItemModel> Servers { get; } = new();

        public MatchingService(IAccountService accountService)
        {
            _accountService = accountService;
        }

        public virtual MatchingGroup GetMatchingGroupBySessionId(string sessionId)
        {
            lock (LockingObject)
            {
                return MatchingGroups.FirstOrDefault(x => x.Members.Contains(sessionId));
            }
        }

        public virtual MatchingGroup GetMatchingGroupByAID(string aid)
        {
            lock (LockingObject)
            {
                var account = _accountService.GetAccountByAID(aid);
                return MatchingGroups.FirstOrDefault(x => x.Members.Contains(account.AccountId));
            }
        }

        public virtual MatchingGroup GetMatchingGroupByRequestId(string requestId)
        {
            lock (LockingObject)
            {
                return MatchingGroups.FirstOrDefault(x => x.GroupInviteRequests.Any(x => x.requestId == requestId));
            }
        }

        public virtual void DeleteMatchingGroupBySessionId(string sessionId)
        {
            lock (LockingObject)
            {
                var findMatchingGroup = MatchingGroups.FirstOrDefault(x => x.SquadLeaderId == sessionId);
                if (findMatchingGroup != null)
                {
                    MatchingGroups.Remove(findMatchingGroup);
                }
            }
        }

        public virtual MatchingGroup CreateMatchingGroupBySessionId(string sessionId)
        {
            var matchGroup = new MatchingGroup()
            {
                MatchingGroupId = MongoID.Generate(false).ToString(),
                Members = new List<string>() { sessionId },
                SquadLeaderId = sessionId
            };
            lock (LockingObject)
            {
                if (!MatchingGroups.Any(x => x.SquadLeaderId == sessionId))
                    MatchingGroups.Add(matchGroup);
            }
            return matchGroup;
        }

        public virtual MatchingGroup SendInviteToUser(string requestId, string mySessionId, string toSessionId)
        {
            var myGroup = GetMatchingGroupBySessionId(mySessionId);
            if (myGroup == null)
                throw new Exception("Unable to find Group");

            if (!myGroup.GroupInviteRequests.Any(x => x.requestId == requestId || x.sessionId == toSessionId))
                myGroup.GroupInviteRequests.Add((requestId, toSessionId));

            return myGroup;
        }

        public virtual MatchingGroup AcceptInviteFromUser(string mySessionId)
        {
            lock (LockingObject)
            {
                var matchingGroup = MatchingGroups.FirstOrDefault(x => x.GroupInviteRequests.Any(invite => invite.sessionId == mySessionId));
                return matchingGroup;
            }
        }

        public virtual bool? CancelInviteToUser(string inviteRequestId)
        {
            lock (LockingObject)
            {
                var matchingGroup = MatchingGroups.FirstOrDefault(x => x.GroupInviteRequests.Any(invite => invite.requestId == inviteRequestId));
                var request = matchingGroup.GroupInviteRequests.FirstOrDefault(invite => invite.requestId == inviteRequestId);
                return matchingGroup.GroupInviteRequests.Remove(request);
            }
        }

        public virtual bool? AddServer(ServerItemModel serverItem)
        {
            lock (LockingObject)
            {
                if (Servers.Any(x => x.Port == serverItem.Port && x.IPAddress == serverItem.IPAddress))
                {
                    return false; // Server already exists
                }
                Servers.Add(serverItem);
            }
            return null;
        }
        public virtual bool? RemoveServer(ServerItemModel serverItem)
        {
            lock (LockingObject)
            {
                Servers.Remove(serverItem);
            }
            return null;
        }
    }
}
