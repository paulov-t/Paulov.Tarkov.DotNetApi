namespace Paulov.TarkovModels.Responses
{
    public sealed class ProfileStatusResponse
    {
        public bool maxPveCountExceeded { get; set; } = false;

        /// <summary>
        /// The profiles are expected by the client in the following format: 0: Scav, 1 PMC
        /// </summary>
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
