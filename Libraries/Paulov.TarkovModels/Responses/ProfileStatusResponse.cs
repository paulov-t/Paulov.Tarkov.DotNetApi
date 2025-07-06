namespace Paulov.TarkovModels.Responses
{
    public sealed class ProfileStatusResponse
    {
        public bool maxPveCountExceeded { get; set; } = false;

        public List<ProfileStatusModel> profiles { get; set; } = new();

        public ProfileStatusResponse() { }

        public ProfileStatusResponse(bool maxPveCountExceeded, List<ProfileStatusModel> profiles)
        {
            this.maxPveCountExceeded = maxPveCountExceeded;
            this.profiles = profiles;
        }
    }
}
