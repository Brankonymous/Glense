namespace Glense.Server
{
    public class Message
    {
        public int messageId {  get; set; }
        public Conversation conversation { get; set; }
        public int conversationId { get; set; }
        public User sender { get; set; }
        public string text { get; set; }
        public DateTime sentAt { get; set; } 
        public bool seen {  get; set; }

    }
}
