using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using DonationService.Data;
using DonationService.Entities;
using Xunit;
using Glense.TestUtilities;

namespace DonationService.IntegrationTests;

public class WalletControllerTests : IClassFixture<CustomDonationServiceFactory>
{
    private readonly HttpClient _client;
    private readonly CustomDonationServiceFactory _factory;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public WalletControllerTests(CustomDonationServiceFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private void SetAuth(Guid userId)
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", JwtTokenHelper.GenerateToken(userId));
    }

    private async Task<Wallet> SeedWallet(Guid userId, decimal balance = 100m)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DonationDbContext>();
        var wallet = new Wallet
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Balance = balance,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Wallets.Add(wallet);
        await db.SaveChangesAsync();
        return wallet;
    }

    [Fact]
    public async Task GetWallet_ExistingWallet_Returns200()
    {
        var userId = Guid.NewGuid();
        SetAuth(userId);
        await SeedWallet(userId, 500m);

        var response = await _client.GetAsync($"/api/wallet/user/{userId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        Assert.Equal(500m, json.GetProperty("balance").GetDecimal());
    }

    [Fact]
    public async Task GetWallet_NotFound_Returns404()
    {
        var userId = Guid.NewGuid();
        SetAuth(userId);

        var response = await _client.GetAsync($"/api/wallet/user/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateWallet_NewWallet_Returns201()
    {
        var userId = Guid.NewGuid();
        SetAuth(userId);

        var response = await _client.PostAsJsonAsync("/api/wallet", new { UserId = userId, InitialBalance = 0m });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        Assert.Equal(userId, json.GetProperty("userId").GetGuid());
    }

    [Fact]
    public async Task CreateWallet_ExistingWallet_Returns200Idempotent()
    {
        var userId = Guid.NewGuid();
        SetAuth(userId);
        await SeedWallet(userId, 100m);

        var response = await _client.PostAsJsonAsync("/api/wallet", new { UserId = userId, InitialBalance = 0m });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task TopUp_AddsBalance()
    {
        var userId = Guid.NewGuid();
        SetAuth(userId);
        await SeedWallet(userId, 100m);

        var response = await _client.PostAsJsonAsync($"/api/wallet/user/{userId}/topup", new { Amount = 50m });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        Assert.Equal(150m, json.GetProperty("balance").GetDecimal());
    }

    [Fact]
    public async Task TopUp_ZeroAmount_Returns400()
    {
        var userId = Guid.NewGuid();
        SetAuth(userId);
        await SeedWallet(userId);

        var response = await _client.PostAsJsonAsync($"/api/wallet/user/{userId}/topup", new { Amount = 0m });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task TopUp_WalletNotFound_Returns404()
    {
        var userId = Guid.NewGuid();
        SetAuth(userId);

        var response = await _client.PostAsJsonAsync($"/api/wallet/user/{Guid.NewGuid()}/topup", new { Amount = 50m });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Withdraw_SubtractsBalance()
    {
        var userId = Guid.NewGuid();
        SetAuth(userId);
        await SeedWallet(userId, 200m);

        var response = await _client.PostAsJsonAsync($"/api/wallet/user/{userId}/withdraw", new { Amount = 50m });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        Assert.Equal(150m, json.GetProperty("balance").GetDecimal());
    }

    [Fact]
    public async Task Withdraw_InsufficientFunds_Returns400()
    {
        var userId = Guid.NewGuid();
        SetAuth(userId);
        await SeedWallet(userId, 10m);

        var response = await _client.PostAsJsonAsync($"/api/wallet/user/{userId}/withdraw", new { Amount = 100m });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Withdraw_ZeroAmount_Returns400()
    {
        var userId = Guid.NewGuid();
        SetAuth(userId);
        await SeedWallet(userId);

        var response = await _client.PostAsJsonAsync($"/api/wallet/user/{userId}/withdraw", new { Amount = 0m });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
