using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Glense.VideoCatalogue.Data;
using Glense.VideoCatalogue.Models;
using Glense.Shared.Messages;
using Xunit;
using Glense.TestUtilities;

namespace VideoCatalogue.IntegrationTests;

public class SubscriptionsControllerTests : IClassFixture<CustomVideoCatalogueFactory>
{
    private readonly HttpClient _client;
    private readonly CustomVideoCatalogueFactory _factory;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public SubscriptionsControllerTests(CustomVideoCatalogueFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private void SetAuth(Guid userId, string username = "subscriber")
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", JwtTokenHelper.GenerateToken(userId, username));
    }

    [Fact]
    public async Task Subscribe_NewSubscription_Returns201()
    {
        var subscriberId = Guid.NewGuid();
        var channelOwnerId = Guid.NewGuid();
        SetAuth(subscriberId);

        var response = await _client.PostAsJsonAsync("/api/subscriptions", new { SubscribedToId = channelOwnerId });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        Assert.Equal(subscriberId, json.GetProperty("subscriberId").GetGuid());
    }

    [Fact]
    public async Task Subscribe_AlreadySubscribed_Returns409()
    {
        var subscriberId = Guid.NewGuid();
        var channelOwnerId = Guid.NewGuid();
        SetAuth(subscriberId);

        await _client.PostAsJsonAsync("/api/subscriptions", new { SubscribedToId = channelOwnerId });
        var response = await _client.PostAsJsonAsync("/api/subscriptions", new { SubscribedToId = channelOwnerId });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Subscribe_PublishesUserSubscribedEvent()
    {
        var subscriberId = Guid.NewGuid();
        var channelOwnerId = Guid.NewGuid();
        SetAuth(subscriberId);

        await _client.PostAsJsonAsync("/api/subscriptions", new { SubscribedToId = channelOwnerId });

        _factory.MockPublishEndpoint.Verify(
            p => p.Publish(
                It.Is<UserSubscribedEvent>(e =>
                    e.SubscriberId == subscriberId && e.ChannelOwnerId == channelOwnerId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Unsubscribe_ExistingSubscription_Returns204()
    {
        var subscriberId = Guid.NewGuid();
        var channelOwnerId = Guid.NewGuid();
        SetAuth(subscriberId);

        await _client.PostAsJsonAsync("/api/subscriptions", new { SubscribedToId = channelOwnerId });

        var request = new HttpRequestMessage(HttpMethod.Delete, "/api/subscriptions")
        {
            Content = JsonContent.Create(new { SubscribedToId = channelOwnerId })
        };
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Unsubscribe_NotSubscribed_Returns404()
    {
        var subscriberId = Guid.NewGuid();
        SetAuth(subscriberId);

        var request = new HttpRequestMessage(HttpMethod.Delete, "/api/subscriptions")
        {
            Content = JsonContent.Create(new { SubscribedToId = Guid.NewGuid() })
        };
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
