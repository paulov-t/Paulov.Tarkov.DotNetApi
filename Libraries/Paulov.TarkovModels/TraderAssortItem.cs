using EFT;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json.Serialization;

namespace Paulov.TarkovModels
{
    public class TraderAssortItem
    {
        private string? _slotId;

        [JsonProperty("_id")]
        [JsonPropertyName("_id")]
        public virtual required MongoID Id { get; set; }

        [JsonProperty("_tpl")]
        [JsonPropertyName("_tpl")]
        public MongoID Template { get; set; }

        [JsonProperty("parentId")]
        [JsonPropertyName("parentId")]
        public string? ParentId { get; set; }

        [JsonProperty("slotId")]
        [JsonPropertyName("slotId")]
        public string? SlotId
        {
            get { return _slotId; }
            set { _slotId = value == null ? null : string.Intern(value); }
        }

        [JsonProperty("location")]
        [JsonPropertyName("location")]
        public JObject? Location { get; set; }

        [JsonProperty("desc")]
        [JsonPropertyName("desc")]
        public string? Desc { get; set; }

        [JsonProperty("upd")]
        [JsonPropertyName("upd")]
        public JObject Upd { get; set; } = new JObject();

        public FlatItem ToFlatItem()
        {
            var flatItem = new FlatItem
            {
                _id = Id,
                _tpl = Template,
                slotId = SlotId,
                //location = Location,
                upd = new UnparsedData() { JToken = Upd },
            };

            if (ParentId != "hideout")
            {
                flatItem.parentId = ParentId;
            }

            if (flatItem.upd != null && flatItem.upd.JObject != null)
            {
                if (flatItem.upd.JObject.Count == 0)
                {
                    flatItem.upd = null;
                }
            }


            return flatItem;
        }
    }
}
