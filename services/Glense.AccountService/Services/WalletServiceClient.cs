using System.Net.Http.Json;

namespace Glense.AccountService.Services
{
    public class WalletServiceClient : IWalletServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<WalletServiceClient> _logger;

        public WalletServiceClient(IHttpClientFactory httpClientFactory, ILogger<WalletServiceClient> logger)
        {
            _httpClient = httpClientFactory.CreateClient("DonationService");
            _logger = logger;
        }

        public async Task<bool> CreateWalletAsync(Guid userId, decimal initialBalance = 0)
        {
            try
            {
                var request = new { UserId = userId, InitialBalance = initialBalance };
                var response = await _httpClient.PostAsJsonAsync("/api/wallet", request);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Wallet created for user {UserId}", userId);
                    return true;
                }

                _logger.LogWarning(
                    "Failed to create wallet for user {UserId}. Status: {StatusCode}",
                    userId, response.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error creating wallet for user {UserId}", userId);
                return false;
            }
        }
    }
}
