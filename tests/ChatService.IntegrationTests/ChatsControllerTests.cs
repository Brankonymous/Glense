using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Glense.ChatService.Data;
using Glense.ChatService.Models;
using Xunit;
using Glense.TestUtilities;

namespace ChatService.IntegrationTests;

public class ChatsControllerTests : IClassFixture<CustomChatServiceFactory>
{
    private readonly HttpClient _client;
    private readonly CustomChatServiceFactory _factory;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public ChatsControllerTests(CustomChatServiceFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private void SetAuth(Guid userId, string username = "testuser")
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", JwtTokenHelper.GenerateToken(userId, username));
    }

    private async Task<Chat> SeedChat(string topic = "Test Chat")
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
        var chat = new Chat
        {
            Id = Guid.NewGuid(),
            Topic = topic,
            CreatedAtUtc = DateTime.UtcNow
        };
        db.Chats.Add(chat);
        await db.SaveChangesAsync();
        return chat;
    }

    [Fact]
    public async Task GetChats_ReturnsPaginated()
    {
        var userId = Guid.NewGuid();
        SetAuth(userId);
        await SeedChat($"PagTest_{Guid.NewGuid():N}");

        var response = await _client.GetAsync("/api/chats?pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        Assert.True(json.TryGetProperty("items", out var items));
        Assert.True(items.GetArrayLength() > 0);
    }

    [Fact]
    public async Task GetChats_CursorBasedPagination()
    {
        var userId = Guid.NewGuid();
        SetAuth(userId);

        // Seed multiple chats
        for (int i = 0; i < 5; i++)
            await SeedChat($"CursorChat_{i}_{Guid.NewGuid():N}");

        // First page
        var response1 = await _client.GetAsync("/api/chats?pageSize=2");
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        var json1 = await response1.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var items1 = json1.GetProperty("items");

        Assert.True(json1.TryGetProperty("nextCursor", out var cursor), "Expected nextCursor property in response");
        Assert.True(cursor.ValueKind != JsonValueKind.Null, "Expected nextCursor to be non-null when more items exist");

        // Second page using cursor
        var response2 = await _client.GetAsync($"/api/chats?pageSize=2&cursor={cursor.GetString()}");
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
    }

    [Fact]
    public async Task CreateChat_Returns201()
    {
        var userId = Guid.NewGuid();
        SetAuth(userId);

        var response = await _client.PostAsJsonAsync("/api/chats", new { Topic = "New Chat Room" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        Assert.Equal("New Chat Room", json.GetProperty("topic").GetString());
    }

    [Fact]
    public async Task CreateChat_NoJwt_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.PostAsJsonAsync("/api/chats", new { Topic = "Unauthorized Chat" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetChat_ExistingChat_ReturnsWithMessageCount()
    {
        var userId = Guid.NewGuid();
        SetAuth(userId);
        var chat = await SeedChat("Detail Chat");

        var response = await _client.GetAsync($"/api/chats/{chat.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        Assert.Equal("Detail Chat", json.GetProperty("topic").GetString());
        Assert.True(json.TryGetProperty("messagesCount", out _));
    }

    [Fact]
    public async Task GetChat_NotFound_Returns404()
    {
        var userId = Guid.NewGuid();
        SetAuth(userId);

        var response = await _client.GetAsync($"/api/chats/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteChat_Returns204()
    {
        var userId = Guid.NewGuid();
        SetAuth(userId);
        var chat = await SeedChat("ToDelete");

        var response = await _client.DeleteAsync($"/api/chats/{chat.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
