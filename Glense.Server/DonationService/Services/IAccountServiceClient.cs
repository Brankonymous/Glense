namespace DonationService.Services;

public interface IAccountServiceClient
{
    /// <summary>
    /// Gets the username for a user. Returns null if the user doesn't exist.
    /// </summary>
    Task<string?> GetUsernameAsync(Guid userId);

    Task CreateDonationNotificationAsync(Guid recipientUserId, string donorUsername, decimal amount, Guid donationId);
}
