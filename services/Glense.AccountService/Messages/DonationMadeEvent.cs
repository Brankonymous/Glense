namespace Glense.Shared.Messages
{
    public class DonationMadeEvent
    {
        public Guid DonorId { get; set; }
        public Guid RecipientId { get; set; }
        public decimal Amount { get; set; }
        public string DonorUsername { get; set; } = string.Empty;
    }
}
