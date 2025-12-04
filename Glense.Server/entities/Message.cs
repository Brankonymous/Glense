namespace Glense.Server
{
    public class Message
    {
        public int messageId { get; set; }
        public Conversation conversation { get; set; } = null!;
        public int conversationId { get; set; }
        public User sender { get; set; } = null!;
        public string text { get; set; } = string.Empty;
        public DateTime sentAt { get; set; }
        public bool seen { get; set; }
    }
}
