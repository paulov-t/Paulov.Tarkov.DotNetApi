using EFT;
using EFT.Communications;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Paulov.TarkovModels.NotificationModels
{
    public sealed class GroupMatchRaidSettingsModel : BaseNotificationModel
    {
        [JsonProperty("raidSettings")]
        [JsonPropertyName("raidSettings")]
        public RaidSettings? RaidSettings { get; set; }

        public GroupMatchRaidSettingsModel(RaidSettings? raidSettings) : base(ENotificationType.GroupMatchRaidSettings)
        {
            RaidSettings = raidSettings ?? throw new ArgumentNullException(nameof(raidSettings), "RaidSettings cannot be null.");
        }
    }
}
