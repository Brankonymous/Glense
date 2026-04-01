using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Glense.ChatService.Data;
using Glense.ChatService.Models;
using Xunit;

namespace ChatService.IntegrationTests;

public class MessageRootControllerTests : IClassFixture<CustomChatServiceFactory>
{
    private readonly HttpClient _client;
    private readonly CustomChatServiceFactory _factory;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public MessageRootControllerTests(CustomChatServiceFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private void SetAuth(Guid userId)
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", JwtTokenHelper.GenerateToken(userId));
    }

    private async Task<(Chat Chat, Message Message)> SeedChatWithMessage()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
        var chat = new Chat
        {
            Id = Guid.NewGuid(),
            Topic = $"MsgRoot_{Guid.NewGuid():N}",
            CreatedAtUtc = DateTime.UtcNow
        };
        db.Chats.Add(chat);

        var msg = new Message
        {
            Id = Guid.NewGuid(),
            ChatId = chat.Id,
            UserId = Guid.NewGuid(),
            Username = "testuser",
            Sender = MessageSender.User,
            Content = "Test message for root controller",
            CreatedAtUtc = DateTime.UtcNow
        };
        db.Messages.Add(msg);
        await db.SaveChangesAsync();
        return (chat, msg);
    }

    [Fact]
    public async Task GetMessage_ExistingMessage_Returns200()
    {
        var userId = Guid.NewGuid();
        SetAuth(userId);
        var (_, msg) = await SeedChatWithMessage();

        var response = await _client.GetAsync($"/api/messages/{msg.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        Assert.Equal(msg.Content, json.GetProperty("content").GetString());
    }

    [Fact]
    public async Task GetMessage_NotFound_Returns404()
    {
        var userId = Guid.NewGuid();
        SetAuth(userId);

        var response = await _client.GetAsync($"/api/messages/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteMessage_Returns204()
    {
        var userId = Guid.NewGuid();
        SetAuth(userId);
        var (_, msg) = await SeedChatWithMessage();

        var response = await _client.DeleteAsync($"/api/messages/{msg.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify it's actually deleted
        var getResponse = await _client.GetAsync($"/api/messages/{msg.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}
