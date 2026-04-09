using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using DonationService.Data;
using Xunit;
using Glense.TestUtilities;

namespace DonationService.IntegrationTests;

public class DonationDbTests : IClassFixture<CustomDonationServiceFactory>
{
    private readonly HttpClient _client;
    private readonly CustomDonationServiceFactory _factory;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public DonationDbTests(CustomDonationServiceFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private void SetAuth(Guid userId)
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", JwtTokenHelper.GenerateToken(userId));
    }

    [Fact]
    public async Task GetWallet_ForNewUser_Returns404()
    {
        var userId = Guid.NewGuid();
        SetAuth(userId);

        var response = await _client.GetAsync($"/api/wallet/user/{userId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetDonations_ForNewUser_ReturnsEmptyList()
    {
        var userId = Guid.NewGuid();
        SetAuth(userId);

        var response = await _client.GetAsync($"/api/donation/donor/{userId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        Assert.Equal(0, json.GetArrayLength());
    }
}
