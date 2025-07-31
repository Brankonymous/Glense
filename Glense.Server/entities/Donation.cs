namespace Glense.Server
{
    public class Donation
    {
        public int donatorId { get; set; }
        public User recipient { get; set; } = null!;
        public int recipientId { get; set; }
        public int amount { get; set; }
        public DateTime donatedAt { get; set; }
    }
}
