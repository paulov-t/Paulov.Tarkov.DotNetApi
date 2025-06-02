using Newtonsoft.Json;

namespace Paulov.Tarkov.WebServer.DOTNET.Models
{
    public class AccountProfileMode
    {
        [JsonProperty("characters")]
        public AccountProfileCharacterSet Characters { get; set; }

        [JsonProperty("socialNetwork")]
        public AccountProfileCharacterSet SocialNetwork { get; set; }

        [JsonProperty("raidConfiguration")]
        public dynamic RaidConfiguration { get; set; }
    }
}
