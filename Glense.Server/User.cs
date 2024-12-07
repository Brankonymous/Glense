namespace Glense.Server
{
    public class User
    {
        public int userId { get; set; }
        public string username { get; set; }
        public string passwordSHA256 { get; set; }
        public string email { get; set; }
        public string profilePictureURL { get; set; }
        public string account { get; set; }
        public DateTime createdAt { get; set; }
        public ICollection<Subscription> Subscriptions { get; set; }
        public ICollection<Subscription> Subscribers { get; set; }
        public ICollection<Donation> Donations { get; set; }
        public ICollection<Conversation> ConversationsStarted { get; set; }
        public ICollection<Conversation> ConversationsInvited { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public ICollection<CommentLikes> CLikes { get; set; }
        public ICollection<VideoLikes> VLikes { get; set; }


    }
}
