using EFT;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Paulov.TarkovModels.GroupingModels
{
    public class PlayerVisualRepresentationModel
    {
        [JsonProperty("Info")]
        public MatchingGroupPlayerInfoModel Info;

        [JsonProperty("Customization")]
        public Dictionary<EBodyModelPart, MongoID> Customization;

        [JsonProperty("Equipment")]
        public PlayerVisualRepresentationModelEquipmentModel Equipment;

        public PlayerVisualRepresentationModel(MatchingGroupPlayerInfoModel info, Dictionary<EBodyModelPart, MongoID> customization, PlayerVisualRepresentationModelEquipmentModel equipment)
        {
            Info = info ?? throw new ArgumentNullException(nameof(info), "Info cannot be null.");
            Customization = customization ?? throw new ArgumentNullException(nameof(customization), "Customization cannot be null.");
            Equipment = equipment ?? throw new ArgumentNullException(nameof(equipment), "Equipment cannot be null.");
        }

        public class PlayerVisualRepresentationModelEquipmentModel
        {
            [JsonProperty("Id")]
            public string Id { get; set; }

            [JsonProperty("Items")]
            public JArray Items { get; set; }

            public PlayerVisualRepresentationModelEquipmentModel(string id, JArray items)
            {
                Id = id ?? throw new ArgumentNullException(nameof(id), "Id cannot be null.");
                Items = items ?? throw new ArgumentNullException(nameof(items), "Items cannot be null.");
            }
        }
    }
}
