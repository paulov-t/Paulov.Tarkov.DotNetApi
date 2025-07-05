using EFT;

namespace Paulov.TarkovModels.GroupingModels
{
    public class MatchingGroup
    {
        public string MatchingGroupId { get; set; } = MongoID.Generate(false);
        public List<string> Members { get; set; } = new List<string>();
        public string SquadLeaderId { get; set; } = MongoID.Generate(false);

    }
}