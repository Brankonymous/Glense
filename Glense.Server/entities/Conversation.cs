namespace Glense.Server
{
    public class Conversation
    {
        public int conversationId { get; set; }
        public User user1 { get; set; } = null!;
        public int user1Id { get; set; }
        public User user2 { get; set; } = null!;
        public int user2Id { get; set; }
        public DateTime createdAt { get; set; }
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
