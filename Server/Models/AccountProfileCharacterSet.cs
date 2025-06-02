using Newtonsoft.Json;

namespace Paulov.Tarkov.WebServer.DOTNET.Models
{
    public class AccountProfileCharacterSet
    {
        [JsonProperty("pmc")]
        public AccountProfileCharacter PMC { get; set; }

        [JsonProperty("scav")]
        public AccountProfileCharacter Scav { get; set; }
    }
}
