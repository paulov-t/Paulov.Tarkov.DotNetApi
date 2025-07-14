using Paulov.TarkovModels.GroupingModels;

namespace Paulov.TarkovServices.Services.Interfaces
{
    public interface IMatchingService
    {
        public List<MatchingGroup> MatchingGroups { get; }
        public MatchingGroup CreateMatchingGroupBySessionId(string sessionId);
        public MatchingGroup GetMatchingGroupByRequestId(string requestId);
        public MatchingGroup GetMatchingGroupBySessionId(string sessionId);
        public MatchingGroup GetMatchingGroupByAID(string aid);
        public void DeleteMatchingGroupBySessionId(string sessionId);
        public MatchingGroup SendInviteToUser(string requestId, string mySessionId, string toSessionId);
        public MatchingGroup AcceptInviteFromUser(string mySessionId);
        public bool? CancelInviteToUser(string inviteRequestId);


    }
}
