using EFT;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Paulov.TarkovModels.GroupingModels
{
    public class MatchingGroupMember
    {
        [JsonProperty("_id")]
        [JsonPropertyName("_id")]
        public string Id { get; set; } = MongoID.Generate(false);

        [JsonProperty("aid")]
        [JsonPropertyName("aid")]
        public string AID { get; set; }

        [JsonProperty("Info")]
        [JsonPropertyName("Info")]
        public MatchingGroupPlayerInfoModel Info { get; set; }

        [JsonProperty("isLeader")]
        [JsonPropertyName("isLeader")]
        public bool IsLeader { get; set; }

        [JsonProperty("isReady")]
        [JsonPropertyName("isReady")]
        public bool IsReady { get; set; }

        [JsonProperty("lookingGroup")]
        public bool LookingForGroup;

        public MatchingGroupMember(string id, string aid, MatchingGroupPlayerInfoModel info, bool isLeader, bool isReady, bool lookingForGroup)
        {
            Id = id;
            AID = aid;
            Info = info;
            IsLeader = isLeader;
            IsReady = isReady;
            LookingForGroup = lookingForGroup;
        }
    }
}
