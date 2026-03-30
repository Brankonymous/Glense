namespace Glense.Shared.Messages
{
    public class UserSubscribedEvent
    {
        public Guid SubscriberId { get; set; }
        public Guid ChannelOwnerId { get; set; }
        public string SubscriberUsername { get; set; } = string.Empty;
    }
}
