
using Newtonsoft.Json;

namespace Paulov.TarkovModels
{
    public class AccountProfileCharacterSet
    {
        [JsonProperty("pmc")]
        public AccountProfileCharacter PMC { get; set; }

        [JsonProperty("scav")]
        public AccountProfileCharacter Scav { get; set; }
    }
}
