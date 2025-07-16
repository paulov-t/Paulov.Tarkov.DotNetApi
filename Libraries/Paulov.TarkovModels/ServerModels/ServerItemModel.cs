using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Paulov.TarkovModels.ServerModels
{
    public class ServerItemModel
    {
        [JsonProperty("ip")]
        [JsonPropertyName("ip")]
        public string IPAddress { get; set; } = "127.0.0.1";

        [JsonProperty("port")]
        [JsonPropertyName("port")]
        public int Port { get; set; } = 17000;

        [JsonProperty("status")]
        [JsonPropertyName("status")]
        public string Status { get; set; } = EServerItemStatus.Offline.ToString();

        [JsonProperty("lastUpdate")]
        [JsonPropertyName("lastUpdate")]
        public DateTime LastUpdate { get; set; }

        [JsonProperty("ping")]
        [JsonPropertyName("ping")]
        public int Ping { get; set; }

        [JsonProperty("isGroupLeader")]
        [JsonPropertyName("isGroupLeader")]
        public bool IsGroupLeader { get; set; }

        public ServerItemModel() { }

        public ServerItemModel(
            string iPAddress
            , int port
            , EServerItemStatus status
            , DateTime lastUpdate
            , int ping
            , bool isGroupLeader
            )
        {
            IPAddress = iPAddress;
            Port = port;
            Status = status.ToString();
            LastUpdate = lastUpdate;
            Ping = ping;
            IsGroupLeader = isGroupLeader;
        }

        public override bool Equals(object? obj)
        {
            if (obj is not ServerItemModel other)
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return IPAddress == other.IPAddress &&
                   Port == other.Port &&
                   IsGroupLeader == other.IsGroupLeader;
        }
    }
}
