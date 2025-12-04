namespace Glense.Server
{
    public class User
    {
        public int userId { get; set; }
        public string username { get; set; } = string.Empty;
        public string passwordSHA256 { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string? profilePictureURL { get; set; }
        public string account { get; set; } = string.Empty;
        public DateTime createdAt { get; set; }
        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
        public ICollection<Subscription> Subscribers { get; set; } = new List<Subscription>();
        public ICollection<Donation> Donations { get; set; } = new List<Donation>();
        public ICollection<Conversation> ConversationsStarted { get; set; } = new List<Conversation>();
        public ICollection<Conversation> ConversationsInvited { get; set; } = new List<Conversation>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<CommentLikes> CLikes { get; set; } = new List<CommentLikes>();
        public ICollection<VideoLikes> VLikes { get; set; } = new List<VideoLikes>();
    }
}
