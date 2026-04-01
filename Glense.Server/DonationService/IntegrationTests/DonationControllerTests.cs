using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using DonationService.Data;
using DonationService.Entities;
using DonationService.Services;
using Glense.Shared.Messages;
using Xunit;

namespace DonationService.IntegrationTests;

public class DonationControllerTests : IClassFixture<CustomDonationServiceFactory>
{
    private readonly HttpClient _client;
    private readonly CustomDonationServiceFactory _factory;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public DonationControllerTests(CustomDonationServiceFactory factory)
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

    private async Task SeedDonation(Guid donorId, Guid recipientId, decimal amount)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DonationDbContext>();
        db.Donations.Add(new Donation
        {
            Id = Guid.NewGuid(),
            DonorUserId = donorId,
            RecipientUserId = recipientId,
            Amount = amount,
            Message = "Test donation",
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task GetDonationsByDonor_ReturnsDonations()
    {
        var donorId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();
        SetAuth(donorId);
        await SeedDonation(donorId, recipientId, 10m);

        var response = await _client.GetAsync($"/api/donation/donor/{donorId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        Assert.True(json.GetArrayLength() > 0);
    }

    [Fact]
    public async Task GetDonationsByRecipient_ReturnsDonations()
    {
        var donorId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();
        SetAuth(recipientId);
        await SeedDonation(donorId, recipientId, 10m);

        var response = await _client.GetAsync($"/api/donation/recipient/{recipientId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        Assert.True(json.GetArrayLength() > 0);
    }

    [Fact]
    public async Task CreateDonation_ValidTransfer_Returns201()
    {
        var donorId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();
        SetAuth(donorId);
        await SeedWallet(donorId, 500m);
        await SeedWallet(recipientId, 0m);

        var request = new
        {
            DonorUserId = donorId,
            RecipientUserId = recipientId,
            Amount = 50m,
            Message = "Keep up the good work!"
        };

        var response = await _client.PostAsJsonAsync("/api/donation", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        Assert.Equal(50m, json.GetProperty("amount").GetDecimal());
    }

    [Fact]
    public async Task CreateDonation_PublishesDonationMadeEvent()
    {
        var donorId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();
        SetAuth(donorId);
        await SeedWallet(donorId, 500m);
        await SeedWallet(recipientId, 0m);

        var request = new
        {
            DonorUserId = donorId,
            RecipientUserId = recipientId,
            Amount = 25m,
            Message = "Thanks!"
        };

        await _client.PostAsJsonAsync("/api/donation", request);

        _factory.MockPublishEndpoint.Verify(
            p => p.Publish(
                It.Is<DonationMadeEvent>(e =>
                    e.DonorId == donorId && e.RecipientId == recipientId && e.Amount == 25m),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateDonation_ZeroAmount_Returns400()
    {
        var donorId = Guid.NewGuid();
        SetAuth(donorId);

        var request = new { DonorUserId = donorId, RecipientUserId = Guid.NewGuid(), Amount = 0m };
        var response = await _client.PostAsJsonAsync("/api/donation", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateDonation_SelfDonation_Returns400()
    {
        var userId = Guid.NewGuid();
        SetAuth(userId);

        var request = new { DonorUserId = userId, RecipientUserId = userId, Amount = 10m };
        var response = await _client.PostAsJsonAsync("/api/donation", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateDonation_InsufficientFunds_Returns400()
    {
        var donorId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();
        SetAuth(donorId);
        await SeedWallet(donorId, 10m);
        await SeedWallet(recipientId, 0m);

        var request = new { DonorUserId = donorId, RecipientUserId = recipientId, Amount = 100m };
        var response = await _client.PostAsJsonAsync("/api/donation", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateDonation_DonorWalletNotFound_Returns404()
    {
        var donorId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();
        SetAuth(donorId);
        // No wallet seeded for donor

        var request = new { DonorUserId = donorId, RecipientUserId = recipientId, Amount = 10m };
        var response = await _client.PostAsJsonAsync("/api/donation", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateDonation_RecipientNotFound_Returns400()
    {
        var donorId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();
        SetAuth(donorId);
        await SeedWallet(donorId, 100m);

        // Mock returns null for unknown recipient
        _factory.MockAccountServiceClient
            .Setup(c => c.GetUsernameAsync(recipientId))
            .ReturnsAsync((string?)null);

        var request = new { DonorUserId = donorId, RecipientUserId = recipientId, Amount = 10m };
        var response = await _client.PostAsJsonAsync("/api/donation", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
