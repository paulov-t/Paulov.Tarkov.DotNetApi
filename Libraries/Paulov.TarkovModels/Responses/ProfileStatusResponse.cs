using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Paulov.TarkovModels.Responses
{
    public class ProfileStatusResponse
    {
        [JsonProperty("maxPveCountExceeded")]
        [JsonPropertyName("maxPveCountExceeded")]
        public bool maxPveCountExceeded { get; set; } = false;

        /// <summary>
        /// The profiles are expected by the client in the following format: 0: Scav, 1 PMC
        /// </summary>
        [JsonProperty("profiles")]
        [JsonPropertyName("profiles")]
        public List<ProfileStatusModel> profiles { get; set; } = new();

        public ProfileStatusResponse() { }

        public ProfileStatusResponse(bool maxPveCountExceeded, List<ProfileStatusModel> profiles)
        {
            this.maxPveCountExceeded = maxPveCountExceeded;
            this.profiles = profiles;
        }

        public ProfileStatusResponse(bool maxPveCountExceeded, ProfileStatusModel pmcStatusModel, ProfileStatusModel scavStatusModel)
        {
            this.maxPveCountExceeded = maxPveCountExceeded;
            this.profiles = new List<ProfileStatusModel>()
            {
                scavStatusModel, pmcStatusModel
            };
        }
    }
}
