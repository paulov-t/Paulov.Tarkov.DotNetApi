using EFT.Communications;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Paulov.TarkovModels.NotificationModels
{
    public sealed class GroupMatchStartGameModel : BaseNotificationModel
    {
        [JsonProperty("groupId")]
        [JsonPropertyName("groupId")]
        public string GroupId { get; set; } = string.Empty;

        public GroupMatchStartGameModel(string groupId) : base(ENotificationType.GroupMatchStartGame)
        {
            GroupId = groupId;
        }
    }
}
