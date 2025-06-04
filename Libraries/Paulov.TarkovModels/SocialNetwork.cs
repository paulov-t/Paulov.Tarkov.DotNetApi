namespace Paulov.TarkovModels
{
    public class SocialNetwork
    {
        public FriendRequest[] FriendRequestInbox { get; set; }
        public FriendRequest[] FriendRequestOutbox { get; set; }
        public List<string> Friends { get; set; }
        public List<Dialogue> Dialogues { get; set; }
    }
}
