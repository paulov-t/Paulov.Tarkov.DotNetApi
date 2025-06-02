using EFT;
using Newtonsoft.Json;

namespace Paulov.Tarkov.WebServer.DOTNET.Models
{
    public class Account
    {
        [JsonProperty("accountId")]
        public string AccountId { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("edition")]
        public string Edition { get; set; } = Profile.THE_UNHEARD_EDITION_VERSION;

        [JsonProperty("modes")]
        public AccountProfileModes Modes { get; set; }

        [JsonProperty("currentMode")]
        public string CurrentMode { get; set; }
    }
}
