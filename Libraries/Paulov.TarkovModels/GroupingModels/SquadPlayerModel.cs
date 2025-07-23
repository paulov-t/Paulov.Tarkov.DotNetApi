using Newtonsoft.Json;

namespace Paulov.TarkovModels.GroupingModels
{
    public class SquadPlayerModel
    {
        [JsonProperty("aid")]
        public string AccountId = string.Empty;

        [JsonProperty("_id")]
        public string Id = string.Empty;

        [JsonProperty("lookingGroup")]
        public bool LookingForGroup;

        [JsonProperty("IsLeader")]
        public bool IsLeader;

        [JsonProperty("IsReady")]
        public bool IsReady;

        [JsonProperty("Info")]
        public MatchingGroupPlayerInfoModel Info;

        [JsonProperty("PlayerVisualRepresentation")]
        public PlayerVisualRepresentationModel PlayerVisualRepresentation;

        public SquadPlayerModel()
        {
            // Default constructor for serialization
        }

        public SquadPlayerModel(string accountId, string id, bool lookingForGroup, bool isLeader, bool isReady, MatchingGroupPlayerInfoModel info, PlayerVisualRepresentationModel playerVisualRepresentation)
        {
            AccountId = accountId ?? throw new ArgumentNullException(nameof(accountId), "AccountId cannot be null.");
            Id = id ?? throw new ArgumentNullException(nameof(id), "Id cannot be null.");
            LookingForGroup = lookingForGroup;
            IsLeader = isLeader;
            IsReady = isReady;
            Info = info ?? throw new ArgumentNullException(nameof(info), "Info cannot be null.");
            PlayerVisualRepresentation = playerVisualRepresentation;
        }
    }
}
