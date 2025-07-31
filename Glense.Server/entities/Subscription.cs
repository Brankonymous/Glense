namespace Glense.Server
{
    public class Subscription
    {
        public User subscriber { get; set; } = null!;
        public int subscriberId { get; set; }
        public User subscribedTo { get; set; } = null!;
        public int subscribedToId { get; set; }
    }
}
