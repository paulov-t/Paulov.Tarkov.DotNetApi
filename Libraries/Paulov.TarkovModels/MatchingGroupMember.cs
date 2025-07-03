using EFT;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Paulov.TarkovModels
{
    public class MatchingGroupMember
    {
        /*
         *        { "_id", _saveProvider.GetPmcProfile(fromAccount).Id.ToString() },
                    { "aid", _saveProvider.GetPmcProfile(fromAccount).AccountId.ToString() },
                    { "Info", JObject.FromObject(_saveProvider.GetPmcProfile(fromAccount).Info) },
                    { "isLeader", true },
                    { "isReady", inLobby },
         */

        [JsonProperty("_id")]
        [JsonPropertyName("_id")]
        public string Id { get; set; } = MongoID.Generate(false);

        [JsonProperty("aid")]
        [JsonPropertyName("aid")]
        public string AID { get; set; }

        [JsonProperty("Info")]
        [JsonPropertyName("Info")]
        public ProfileInfoDescriptor Info { get; set; } = new ProfileInfoDescriptor();

        [JsonProperty("isLeader")]
        [JsonPropertyName("isLeader")]
        public bool IsLeader { get; set; }

        [JsonProperty("isReady")]
        [JsonPropertyName("isReady")]
        public bool IsReady { get; set; }

        public MatchingGroupMember(string id, string aid, ProfileInfoDescriptor info, bool isLeader, bool isReady)
        {
            Id = id;
            AID = aid;
            Info = info;
            IsLeader = isLeader;
            IsReady = isReady;
        }
    }
}
