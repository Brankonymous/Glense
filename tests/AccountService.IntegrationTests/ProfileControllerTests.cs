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

public class ProfileControllerTests : IClassFixture<CustomAccountServiceFactory>
{
    private readonly HttpClient _client;
    private readonly CustomAccountServiceFactory _factory;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public ProfileControllerTests(CustomAccountServiceFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private void SetAuth(Guid userId, string username = "testuser", string email = "test@example.com")
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", JwtTokenHelper.GenerateToken(userId, username, email));
    }

    private async Task<User> SeedUser(string? username = null, bool isActive = true)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AccountDbContext>();
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = username ?? $"user_{Guid.NewGuid():N}",
            Email = $"{Guid.NewGuid():N}@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            AccountType = "user",
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    [Fact]
    public async Task SearchUsers_ReturnsMatchingActiveUsers()
    {
        var user = await SeedUser("searchable_user_abc");

        var response = await _client.GetAsync("/api/profile/search?q=searchable_user_abc");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        Assert.True(json.GetArrayLength() > 0);
    }

    [Fact]
    public async Task SearchUsers_EmptyQuery_ReturnsAllActiveUsers()
    {
        await SeedUser();

        var response = await _client.GetAsync("/api/profile/search");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        Assert.True(json.GetArrayLength() > 0);
    }

    [Fact]
    public async Task SearchUsers_InactiveUsersExcluded()
    {
        var inactiveUser = await SeedUser("inactive_xyz_hidden", isActive: false);

        var response = await _client.GetAsync("/api/profile/search?q=inactive_xyz_hidden");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        Assert.Equal(0, json.GetArrayLength());
    }

    [Fact]
    public async Task GetMe_WithJwt_ReturnsCurrentUser()
    {
        var user = await SeedUser();
        SetAuth(user.Id, user.Username, user.Email);

        var response = await _client.GetAsync("/api/profile/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        Assert.Equal(user.Username, json.GetProperty("username").GetString());
    }

    [Fact]
    public async Task GetMe_NoJwt_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.GetAsync("/api/profile/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ExistingUser_ReturnsUser()
    {
        var user = await SeedUser();

        var response = await _client.GetAsync($"/api/profile/{user.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        Assert.Equal(user.Username, json.GetProperty("username").GetString());
    }

    [Fact]
    public async Task GetById_NonExistent_Returns404()
    {
        var response = await _client.GetAsync($"/api/profile/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetById_InactiveUser_Returns404()
    {
        var user = await SeedUser(isActive: false);

        var response = await _client.GetAsync($"/api/profile/{user.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateProfile_UpdateUsername_Succeeds()
    {
        var user = await SeedUser();
        SetAuth(user.Id, user.Username, user.Email);
        var newUsername = $"updated_{Guid.NewGuid():N}";

        var response = await _client.PutAsJsonAsync("/api/profile/me", new { Username = newUsername });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        Assert.Equal(newUsername, json.GetProperty("username").GetString());
    }

    [Fact]
    public async Task UpdateProfile_DuplicateUsername_Returns400()
    {
        var user1 = await SeedUser();
        var user2 = await SeedUser();
        SetAuth(user2.Id, user2.Username, user2.Email);

        var response = await _client.PutAsJsonAsync("/api/profile/me", new { Username = user1.Username });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateProfile_NoJwt_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.PutAsJsonAsync("/api/profile/me", new { Username = "anything" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteAccount_SoftDeletes()
    {
        var user = await SeedUser();
        SetAuth(user.Id, user.Username, user.Email);

        var response = await _client.DeleteAsync("/api/profile/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify user is now inactive (not found via public endpoint)
        var getResponse = await _client.GetAsync($"/api/profile/{user.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}
