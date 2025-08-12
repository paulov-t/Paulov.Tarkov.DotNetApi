using Newtonsoft.Json;

namespace Paulov.TarkovModels
{
    public class ProfileEditionModels
    {
        public ProfileEditionModels() { }

        [JsonProperty("Standard")]
        public ProfileEditionModel Standard { get; set; } = new ProfileEditionModel();

        [JsonProperty("Left Behind")]
        public ProfileEditionModel Left_Behind { get; set; } = new ProfileEditionModel();

        [JsonProperty("Prepare To Escape")]
        public ProfileEditionModel Prepare_To_Escape { get; set; } = new ProfileEditionModel();

        [JsonProperty("Edge Of Darkness")]
        public ProfileEditionModel Edge_Of_Darkness { get; set; } = new ProfileEditionModel();

        [JsonProperty("Unheard")]
        public ProfileEditionModel Unheard { get; set; } = new ProfileEditionModel();

        [JsonProperty("Tournament")]
        public ProfileEditionModel Tournament { get; set; } = new ProfileEditionModel();

    }

    public class ProfileEditionModel
    {
        [JsonProperty("bear")]
        public ProfileEditionModelCharacterOption Bear { get; set; } = new ProfileEditionModelCharacterOption();

        [JsonProperty("usec")]
        public ProfileEditionModelCharacterOption Usec { get; set; } = new ProfileEditionModelCharacterOption();
    }

    public class ProfileEditionModelCharacterOption
    {
        [JsonProperty("character")]
        public AccountProfileCharacter Character { get; set; } = new AccountProfileCharacter();
    }
}
