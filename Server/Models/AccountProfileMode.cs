using Newtonsoft.Json;

namespace Paulov.Tarkov.WebServer.DOTNET.Models
{
    /// <summary>
    /// Represents the configuration and settings for an account profile, including associated characters, social
    /// network information, and raid-specific configurations.
    /// </summary>
    /// <remarks>This class encapsulates various aspects of an account profile, such as the set of characters
    /// linked to the profile, social network details, and dynamic raid configurations. It is typically used to manage
    /// and serialize account-related data.</remarks>
    public class AccountProfileMode
    {
        /// <summary>
        /// Gets or sets the set of characters associated with the account profile.
        /// </summary>
        [JsonProperty("characters")]
        public AccountProfileCharacterSet Characters { get; set; } = new AccountProfileCharacterSet();

        /// <summary>
        /// Gets or sets the social network information associated with the account profile.
        /// </summary>
        [JsonProperty("socialNetwork")]
        public AccountProfileCharacterSet SocialNetwork { get; set; } = new AccountProfileCharacterSet();

        [JsonProperty("raidConfiguration")]
        public dynamic RaidConfiguration { get; set; }
    }
}
