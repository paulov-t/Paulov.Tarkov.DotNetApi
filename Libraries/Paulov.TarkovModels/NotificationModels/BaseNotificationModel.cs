using EFT;
using EFT.Communications;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Paulov.TarkovModels.NotificationModels
{
    public abstract class BaseNotificationModel
    {
        [JsonProperty("type")]
        [JsonPropertyName("type")]
        public string Type { get; set; } = "groupMatchRaidNotReady";

        [JsonProperty("eventId")]
        [JsonPropertyName("eventId")]
        public string EventId { get; set; } = MongoID.Generate().ToString();

        public BaseNotificationModel()
        {
            // Default constructor
        }

        public BaseNotificationModel(ENotificationType notificationType)
        {
            // Get the BSGNamedAttribute for the ENotificationType enum value notificationType
            var attribute = notificationType.GetType()
                .GetField(notificationType.ToString())
                ?.GetCustomAttributes(typeof(BSGNamedAttribute), false)
                .FirstOrDefault() as BSGNamedAttribute;

            if (attribute == null)
            {
                throw new ArgumentException($"Notification type {notificationType} does not have a BSGNamedAttribute.", nameof(notificationType));
            }

            Type = attribute.Name ?? throw new ArgumentNullException(nameof(attribute), "Type cannot be null.");
        }

    }
}
