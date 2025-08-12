using Newtonsoft.Json;
using Paulov.TarkovModels.GroupingModels;
using System.Text.Json.Serialization;

namespace Paulov.TarkovModels.Responses
{
    public sealed class MatchGroupStatusResponse
    {
        [JsonProperty("players")]
        [JsonPropertyName("players")]
        public List<SquadPlayerModel> Players;

        [JsonProperty("maxPveCountExceeded")]
        [JsonPropertyName("maxPveCountExceeded")]
        public bool LimitedServersAvailability;

        public MatchGroupStatusResponse(List<SquadPlayerModel> players, bool limitedServersAvailability)
        {
            Players = players ?? throw new ArgumentNullException(nameof(players), "Players cannot be null.");
            LimitedServersAvailability = limitedServersAvailability;
        }
    }
}
