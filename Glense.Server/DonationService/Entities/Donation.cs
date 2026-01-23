namespace DonationService.Entities;

public class Donation
{
    public Guid Id { get; set; }
    public Guid DonorUserId { get; set; }
    public Guid RecipientUserId { get; set; }
    public decimal Amount { get; set; }
    public string? Message { get; set; }
    public DateTime CreatedAt { get; set; }
}
