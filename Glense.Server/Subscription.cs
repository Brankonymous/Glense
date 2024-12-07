namespace Glense.Server
{
    public class Subscription
    {
        public User subscriber {  get; set; }
        public int subscriberId {  get; set; }
        public User subscribedTo { get; set; }
        public int subscribedToId { get; set; }
    }
}
