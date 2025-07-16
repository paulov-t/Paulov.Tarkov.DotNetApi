using Paulov.TarkovModels.GroupingModels;
using Paulov.TarkovModels.ServerModels;

namespace Paulov.TarkovServices.Services.Interfaces
{
    public interface IMatchingService
    {
        public List<MatchingGroup> MatchingGroups { get; }
        public List<ServerItemModel> Servers { get; }
        public MatchingGroup CreateMatchingGroupBySessionId(string sessionId);
        public MatchingGroup GetMatchingGroupByRequestId(string requestId);
        public MatchingGroup GetMatchingGroupBySessionId(string sessionId);
        public MatchingGroup GetMatchingGroupByAID(string aid);
        public void DeleteMatchingGroupBySessionId(string sessionId);
        public MatchingGroup SendInviteToUser(string requestId, string mySessionId, string toSessionId);
        public MatchingGroup AcceptInviteFromUser(string mySessionId);
        public bool? CancelInviteToUser(string inviteRequestId);

        public bool? AddServer(ServerItemModel serverItem);
        public bool? RemoveServer(ServerItemModel serverItem);

    }
}
