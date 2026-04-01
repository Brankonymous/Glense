using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Glense.AccountService.Data;
using Glense.AccountService.Models;
using Xunit;

namespace AccountService.IntegrationTests;

public class NotificationControllerTests : IClassFixture<CustomAccountServiceFactory>
{
    private readonly HttpClient _client;
    private readonly CustomAccountServiceFactory _factory;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public NotificationControllerTests(CustomAccountServiceFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private void SetAuth(Guid userId, string username = "testuser")
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", JwtTokenHelper.GenerateToken(userId, username));
    }

    private async Task<(User User, List<Notification> Notifications)> SeedUserWithNotifications(int count = 3, bool allRead = false)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AccountDbContext>();
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = $"notifuser_{Guid.NewGuid():N}",
            Email = $"{Guid.NewGuid():N}@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        db.Users.Add(user);

        var notifications = new List<Notification>();
        for (int i = 0; i < count; i++)
        {
            var n = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Title = $"Test Notification {i}",
                Message = $"Message {i}",
                Type = "system",
                IsRead = allRead || (i % 2 == 0),
                CreatedAt = DateTime.UtcNow.AddMinutes(-i)
            };
            notifications.Add(n);
            db.Notifications.Add(n);
        }

        await db.SaveChangesAsync();
        return (user, notifications);
    }

    [Fact]
    public async Task GetNotifications_ReturnsPaginated()
    {
        var (user, _) = await SeedUserWithNotifications(5);
        SetAuth(user.Id, user.Username);

        var response = await _client.GetAsync("/api/notification?take=3");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        Assert.True(json.GetArrayLength() <= 3);
    }

    [Fact]
    public async Task GetNotifications_FilterByIsRead()
    {
        var (user, notifications) = await SeedUserWithNotifications(4);
        SetAuth(user.Id, user.Username);

        var response = await _client.GetAsync("/api/notification?isRead=false");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        foreach (var item in json.EnumerateArray())
        {
            Assert.False(item.GetProperty("isRead").GetBoolean());
        }
    }

    [Fact]
    public async Task GetUnreadCount_ReturnsCount()
    {
        var (user, notifications) = await SeedUserWithNotifications(4);
        SetAuth(user.Id, user.Username);
        var expectedUnread = notifications.Count(n => !n.IsRead);

        var response = await _client.GetAsync("/api/notification/unread-count");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        Assert.Equal(expectedUnread, json.GetProperty("count").GetInt32());
    }

    [Fact]
    public async Task MarkAsRead_ExistingNotification_Succeeds()
    {
        var (user, notifications) = await SeedUserWithNotifications(1);
        SetAuth(user.Id, user.Username);
        var notifId = notifications.First().Id;

        var response = await _client.PutAsync($"/api/notification/{notifId}/read", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task MarkAsRead_WrongNotification_Returns404()
    {
        var (user, _) = await SeedUserWithNotifications(1);
        SetAuth(user.Id, user.Username);

        var response = await _client.PutAsync($"/api/notification/{Guid.NewGuid()}/read", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task MarkAllAsRead_Succeeds()
    {
        var (user, _) = await SeedUserWithNotifications(3);
        SetAuth(user.Id, user.Username);

        var response = await _client.PutAsync("/api/notification/read-all", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify all are read
        var countResponse = await _client.GetAsync("/api/notification/unread-count");
        var json = await countResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        Assert.Equal(0, json.GetProperty("count").GetInt32());
    }
}
