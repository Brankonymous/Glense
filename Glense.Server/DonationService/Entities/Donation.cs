namespace DonationService.Entities;

public class Donation
{
    public Guid Id { get; set; }
    public int DonorUserId { get; set; }
    public int RecipientUserId { get; set; }
    public decimal Amount { get; set; }
    public string? Message { get; set; }
    public DateTime CreatedAt { get; set; }
}
