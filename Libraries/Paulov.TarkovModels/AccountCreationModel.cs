using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Paulov.TarkovModels
{
    public class AccountCreationModel
    {
        [JsonPropertyName("username")]
        [JsonProperty("username")]
        public string? Username { get; set; } // e.g., "player1", "gamer123", "tarkov_fan"

        [JsonPropertyName("password")]
        [JsonProperty("password")]
        public string? Password { get; set; } // e.g., "12345678", "password", "qwerty1234"

        [JsonPropertyName("email")]
        [JsonProperty("email")]
        public string? Email { get; set; } // e.g., ""

        [JsonPropertyName("language")]
        [JsonProperty("language")]
        public string? Language { get; set; } // e.g., "en", "ru"

        [JsonPropertyName("region")]
        [JsonProperty("region")]
        public string? Region { get; set; } // e.g., "EU", "US", "RU"

        [JsonPropertyName("edition")]
        [JsonProperty("edition")]
        public string? Edition { get; set; } // e.g., "Standard", "Left Behind", "Prepare To Escape", "Edge Of Darkness", "Unheard", "Tournament"

    }
}
