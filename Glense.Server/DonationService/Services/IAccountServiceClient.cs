namespace DonationService.Services;

public interface IAccountServiceClient
{
    /// <summary>
    /// Gets the username for a user via HTTP. Returns null if the user doesn't exist.
    /// This is kept as HTTP for synchronous profile validation during donations.
    /// </summary>
    Task<string?> GetUsernameAsync(Guid userId);
}
