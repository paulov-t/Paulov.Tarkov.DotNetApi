using Newtonsoft.Json;

namespace Paulov.TarkovModels.NotificationModels
{
    public sealed class UserConfirmedModel : BaseNotificationModel
    {
        [JsonProperty("profileid")]
        public string ProfileId { get; set; }
        [JsonProperty("profileToken")]
        public string ProfileToken { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("ip")]
        public string Ip { get; set; }
        [JsonProperty("port")]
        public string Port { get; set; }
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

        public UserConfirmedModel() : base(EFT.Communications.ENotificationType.UserConfirmed)
        {

        }

        public UserConfirmedModel(string profileId, string profileToken, string status, string ip, string port, string sid, string version, string location, string raidMode, string mode, string shortId, List<string> additionalInfo) : base(EFT.Communications.ENotificationType.UserConfirmed)
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
