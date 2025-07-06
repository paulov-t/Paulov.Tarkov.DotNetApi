using EFT;
using Newtonsoft.Json;

namespace Paulov.TarkovModels
{
    public class ProfileStatusModel
    {
        [JsonProperty("profileid")]
        public string ProfileId { get; set; } = new MongoID().ToString();
        [JsonProperty("profileToken")]
        public string ProfileToken { get; set; } = Guid.NewGuid().ToString();
        [JsonProperty("status")]
        public string Status { get; set; } = "Free";
        [JsonProperty("ip")]
        public string Ip { get; set; } = "";
        [JsonProperty("port")]
        public string Port { get; set; } = "0";
        [JsonProperty("sid")]
        public string Sid { get; set; }
        [JsonProperty("version")]
        public string Version { get; set; }
        [JsonProperty("location")]
        public string Location { get; set; }
        [JsonProperty("raidMode")]
        public string RaidMode { get; set; }
        [JsonProperty("mode")]
        public string Mode { get; set; }
        [JsonProperty("shortId")]
        public string ShortId { get; set; }
        [JsonProperty("additional_info")]
        public List<string> AdditionalInfo { get; set; } = new List<string>();

        public ProfileStatusModel()
        {

            AdditionalInfo = new List<string>();
        }

        public ProfileStatusModel(string profileId, string status, string ip, string port)
        {
            ProfileId = profileId;
            ProfileToken = Guid.NewGuid().ToString();
            Status = status;
            Ip = ip;
            Port = port;
            //Sid = sid;
            //Version = version;
            //Location = location;
            //RaidMode = raidMode;
            //Mode = mode;
            //ShortId = shortId;
            //AdditionalInfo = additionalInfo;
        }

        public ProfileStatusModel(string profileId, string profileToken, string status, string ip, string port, string sid, string version, string location, string raidMode, string mode, string shortId, List<string> additionalInfo)
        {
            ProfileId = profileId;
            ProfileToken = profileToken;
            Status = status;
            Ip = ip;
            Port = port;
            Sid = sid;
            Version = version;
            Location = location;
            RaidMode = raidMode;
            Mode = mode;
            ShortId = shortId;
            AdditionalInfo = additionalInfo;
        }
    }
}
