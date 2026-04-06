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

public class MessagesControllerTests : IClassFixture<CustomChatServiceFactory>
{
    private readonly HttpClient _client;
    private readonly CustomChatServiceFactory _factory;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public MessagesControllerTests(CustomChatServiceFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private void SetAuth(Guid userId, string username = "testuser")
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", JwtTokenHelper.GenerateToken(userId, username));
    }

    private async Task<Chat> SeedChat()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
        var chat = new Chat
        {
            Id = Guid.NewGuid(),
            Topic = $"MsgTest_{Guid.NewGuid():N}",
            CreatedAtUtc = DateTime.UtcNow
        };
        db.Chats.Add(chat);
        await db.SaveChangesAsync();
        return chat;
    }

    private async Task<Message> SeedMessage(Guid chatId, Guid userId, string content = "Hello")
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
        var msg = new Message
        {
            Id = Guid.NewGuid(),
            ChatId = chatId,
            UserId = userId,
            Username = "testuser",
            Sender = MessageSender.User,
            Content = content,
            CreatedAtUtc = DateTime.UtcNow
        };
        db.Messages.Add(msg);
        await db.SaveChangesAsync();
        return msg;
    }

    [Fact]
    public async Task GetMessages_ReturnsPaginatedMessages()
    {
        var userId = Guid.NewGuid();
        SetAuth(userId);
        var chat = await SeedChat();
        await SeedMessage(chat.Id, userId, "Message 1");
        await SeedMessage(chat.Id, userId, "Message 2");

        var response = await _client.GetAsync($"/api/chats/{chat.Id}/messages?pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var items = json.GetProperty("items");
        Assert.True(items.GetArrayLength() >= 2);
    }

    [Fact]
    public async Task GetMessages_CursorPagination()
    {
        var userId = Guid.NewGuid();
        SetAuth(userId);
        var chat = await SeedChat();
        for (int i = 0; i < 5; i++)
            await SeedMessage(chat.Id, userId, $"Msg {i}");

        var response1 = await _client.GetAsync($"/api/chats/{chat.Id}/messages?pageSize=2");
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        var json1 = await response1.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);

        Assert.True(json1.TryGetProperty("nextCursor", out var cursor), "Expected nextCursor property in response");
        Assert.True(cursor.ValueKind != JsonValueKind.Null, "Expected nextCursor to be non-null when more items exist");

        var response2 = await _client.GetAsync($"/api/chats/{chat.Id}/messages?pageSize=2&cursor={cursor.GetString()}");
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
    }

    [Fact]
    public async Task CreateMessage_UserSender_ReturnsCreated()
    {
        var userId = Guid.NewGuid();
        SetAuth(userId, "msguser");
        var chat = await SeedChat();

        var response = await _client.PostAsJsonAsync($"/api/chats/{chat.Id}/messages",
            new { Sender = "user", Content = "Hello world!" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        Assert.Equal("Hello world!", json.GetProperty("content").GetString());
        Assert.Equal("user", json.GetProperty("sender").GetString());
    }

    [Fact]
    public async Task CreateMessage_SystemSender_ReturnsCreated()
    {
        var userId = Guid.NewGuid();
        SetAuth(userId);
        var chat = await SeedChat();

        var response = await _client.PostAsJsonAsync($"/api/chats/{chat.Id}/messages",
            new { Sender = "system", Content = "System notification" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        Assert.Equal("system", json.GetProperty("sender").GetString());
    }

    [Fact]
    public async Task CreateMessage_ChatNotFound_Returns404()
    {
        var userId = Guid.NewGuid();
        SetAuth(userId);

        var response = await _client.PostAsJsonAsync($"/api/chats/{Guid.NewGuid()}/messages",
            new { Sender = "user", Content = "Ghost message" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
