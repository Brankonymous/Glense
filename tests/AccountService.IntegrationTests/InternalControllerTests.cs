using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Glense.AccountService.Data;
using Glense.AccountService.Models;
using Xunit;
using Glense.TestUtilities;

namespace AccountService.IntegrationTests;

public class InternalControllerTests : IClassFixture<CustomAccountServiceFactory>
{
    private readonly HttpClient _client;
    private readonly CustomAccountServiceFactory _factory;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public InternalControllerTests(CustomAccountServiceFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private void SetAuth(Guid userId)
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", JwtTokenHelper.GenerateToken(userId));
    }

    private async Task<User> SeedUser()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AccountDbContext>();
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = $"internal_{Guid.NewGuid():N}",
            Email = $"{Guid.NewGuid():N}@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    [Fact]
    public async Task CreateNotification_ReturnsNotificationDto()
    {
        var user = await SeedUser();
        SetAuth(user.Id);

        var request = new
        {
            UserId = user.Id,
            Title = "Test Notification",
            Message = "This is a test",
            Type = "system"
        };

        var response = await _client.PostAsJsonAsync("/api/internal/notifications", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        Assert.Equal("Test Notification", json.GetProperty("title").GetString());
        Assert.Equal("system", json.GetProperty("type").GetString());
    }
}
