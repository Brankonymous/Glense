using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace AccountService.IntegrationTests;

public class AuthControllerTests : IClassFixture<CustomAccountServiceFactory>
{
    private readonly HttpClient _client;
    private readonly CustomAccountServiceFactory _factory;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public AuthControllerTests(CustomAccountServiceFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_NewUser_Returns200WithToken()
    {
        var request = new
        {
            Username = $"newuser_{Guid.NewGuid():N}",
            Email = $"new_{Guid.NewGuid():N}@example.com",
            Password = "Password123!"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        Assert.True(json.TryGetProperty("token", out var token));
        Assert.False(string.IsNullOrEmpty(token.GetString()));
        Assert.True(json.TryGetProperty("user", out var user));
        Assert.Equal(request.Username, user.GetProperty("username").GetString());
    }

    [Fact]
    public async Task Register_DuplicateUsername_Returns400()
    {
        var username = $"dupuser_{Guid.NewGuid():N}";
        var request1 = new { Username = username, Email = $"a_{Guid.NewGuid():N}@example.com", Password = "Password123!" };
        var request2 = new { Username = username, Email = $"b_{Guid.NewGuid():N}@example.com", Password = "Password123!" };

        await _client.PostAsJsonAsync("/api/auth/register", request1);
        var response = await _client.PostAsJsonAsync("/api/auth/register", request2);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns400()
    {
        var email = $"dup_{Guid.NewGuid():N}@example.com";
        var request1 = new { Username = $"user1_{Guid.NewGuid():N}", Email = email, Password = "Password123!" };
        var request2 = new { Username = $"user2_{Guid.NewGuid():N}", Email = email, Password = "Password123!" };

        await _client.PostAsJsonAsync("/api/auth/register", request1);
        var response = await _client.PostAsJsonAsync("/api/auth/register", request2);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_ValidCredentials_Returns200WithToken()
    {
        var username = $"loginuser_{Guid.NewGuid():N}";
        var password = "Password123!";
        var registerRequest = new { Username = username, Email = $"{username}@example.com", Password = password };
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new { UsernameOrEmail = username, Password = password };
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        Assert.True(json.TryGetProperty("token", out var token));
        Assert.False(string.IsNullOrEmpty(token.GetString()));
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        var username = $"wrongpw_{Guid.NewGuid():N}";
        var registerRequest = new { Username = username, Email = $"{username}@example.com", Password = "Password123!" };
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new { UsernameOrEmail = username, Password = "WrongPassword!" };
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_NonExistentUser_Returns401()
    {
        var loginRequest = new { UsernameOrEmail = "nonexistent_user_xyz", Password = "Password123!" };
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
