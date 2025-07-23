using EFT.Communications;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Paulov.TarkovModels.NotificationModels
{
    public sealed class GroupMatchRaidNotReadyModel : BaseNotificationModel
    {
        [JsonProperty("aid")]
        [JsonPropertyName("aid")]
        public string AID { get; set; }

        public GroupMatchRaidNotReadyModel(string aid) : base(ENotificationType.GroupMatchRaidNotReady)
        {
            AID = aid;
        }
    }
}
