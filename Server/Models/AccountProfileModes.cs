using Newtonsoft.Json;

namespace Paulov.Tarkov.WebServer.DOTNET.Models
{
    public class AccountProfileModes
    {
        [JsonProperty("regular")]
        public AccountProfileMode Regular { get; set; }

        [JsonProperty("pve")]
        public AccountProfileMode PVE { get; set; }

        [JsonProperty("arena")]
        public AccountProfileMode Arena { get; set; }
    }
}
