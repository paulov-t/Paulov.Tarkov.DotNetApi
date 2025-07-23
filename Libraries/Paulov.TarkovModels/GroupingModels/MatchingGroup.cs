using EFT;

namespace Paulov.TarkovModels.GroupingModels
{
    public class MatchingGroup
    {
        public string MatchingGroupId { get; set; } = MongoID.Generate(false);
        public string SquadLeaderId { get; set; } = MongoID.Generate(false);
        public List<string> Members { get; set; } = new();
        public List<(string requestId, string sessionId)> GroupInviteRequests { get; } = new();

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;

            if (ReferenceEquals(this, obj))
                return true;

            var other = obj as MatchingGroup;
            if (other?.SquadLeaderId == SquadLeaderId) return true;

            return base.Equals(obj);
        }
    }
}