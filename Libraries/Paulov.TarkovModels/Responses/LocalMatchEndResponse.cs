using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Paulov.TarkovModels.Responses
{
    public sealed class LocalMatchEndResponse
    {
        [JsonProperty("serverId")]
        public string ServerId { get; set; }

        [JsonProperty("results")]
        public JObject Results { get; set; }
    }
}
