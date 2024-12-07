namespace Glense.Server
{
    public class Donation
    {
        public int donatorId { get; set; }
        public User recipient { get; set; }
        public int recipientId { get; set; }
        public int amount { get; set; }
        public DateTime donatedAt { get; set; }
    }
}
