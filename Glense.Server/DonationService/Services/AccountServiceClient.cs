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

        var apiKey = Environment.GetEnvironmentVariable("INTERNAL_API_KEY");
        if (!string.IsNullOrEmpty(apiKey))
        {
            _httpClient.DefaultRequestHeaders.Add("X-Internal-Api-Key", apiKey);
        }
    }

    public async Task<string?> GetUsernameAsync(Guid userId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/profile/{userId}");

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
            return json.TryGetProperty("username", out var username) ? username.GetString() : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get username for user {UserId} from Account Service", userId);
            return null;
        }
    }
}
