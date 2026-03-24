using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace DonationService.Services;

public class AccountServiceClient : IAccountServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AccountServiceClient> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public AccountServiceClient(IHttpClientFactory httpClientFactory, ILogger<AccountServiceClient> logger)
    {
        _httpClient = httpClientFactory.CreateClient("AccountService");
        _logger = logger;
    }

    public async Task<string?> GetUsernameAsync(Guid userId)
    {
        var response = await _httpClient.GetAsync($"/api/profile/{userId}");

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        return json.TryGetProperty("username", out var username) ? username.GetString() : null;
    }

    public async Task CreateDonationNotificationAsync(
        Guid recipientUserId,
        string donorUsername,
        decimal amount,
        Guid donationId)
    {
        var request = new
        {
            UserId = recipientUserId,
            Title = "New Donation!",
            Message = $"{donorUsername} donated ${amount:F2} to you!",
            Type = "donation",
            RelatedEntityId = donationId
        };

        var response = await _httpClient.PostAsJsonAsync("/api/internal/notifications", request);
        response.EnsureSuccessStatusCode();
    }
}
