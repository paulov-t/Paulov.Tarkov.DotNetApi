namespace Paulov.TarkovModels
{
    public class SocialNetwork
    {
        public List<FriendRequest> FriendRequestInbox { get; set; } = new List<FriendRequest>();
        public List<FriendRequest> FriendRequestOutbox { get; set; } = new List<FriendRequest>();
        public List<string> Friends { get; set; } = new List<string>();
        public List<Dialogue> Dialogues { get; set; } = new List<Dialogue>();
    }
}
