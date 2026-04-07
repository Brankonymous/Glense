using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Glense.VideoCatalogue.Data;
using Glense.VideoCatalogue.Models;
using Xunit;
using Glense.TestUtilities;

namespace VideoCatalogue.IntegrationTests;

public class VideoLikesControllerTests : IClassFixture<CustomVideoCatalogueFactory>
{
    private readonly HttpClient _client;
    private readonly CustomVideoCatalogueFactory _factory;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public VideoLikesControllerTests(CustomVideoCatalogueFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private void SetAuth(Guid userId)
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", JwtTokenHelper.GenerateToken(userId));
    }

    private async Task<Videos> SeedVideo()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<VideoCatalogueDbContext>();
        var video = new Videos
        {
            Id = Guid.NewGuid(),
            Title = $"LikeTest_{Guid.NewGuid():N}",
            VideoUrl = "test.mp4",
            UploadDate = DateTime.UtcNow,
            UploaderId = Guid.NewGuid()
        };
        db.Videos.Add(video);
        await db.SaveChangesAsync();
        return video;
    }

    [Fact]
    public async Task GetUserLike_NoPriorLike_ReturnsNull()
    {
        var userId = Guid.NewGuid();
        var video = await SeedVideo();
        SetAuth(userId);

        var response = await _client.GetAsync($"/api/videolikes/{video.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        Assert.True(json.GetProperty("liked").ValueKind == JsonValueKind.Null);
    }

    [Fact]
    public async Task Like_NewLike_IncrementsCount()
    {
        var userId = Guid.NewGuid();
        var video = await SeedVideo();
        SetAuth(userId);

        var response = await _client.PostAsJsonAsync("/api/videolikes", new { VideoId = video.Id, IsLiked = true });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        Assert.Equal(1, json.GetProperty("likeCount").GetInt32());
    }

    [Fact]
    public async Task Like_SwitchVote_AdjustsCounts()
    {
        var userId = Guid.NewGuid();
        var video = await SeedVideo();
        SetAuth(userId);

        // Like first
        await _client.PostAsJsonAsync("/api/videolikes", new { VideoId = video.Id, IsLiked = true });

        // Switch to dislike
        var response = await _client.PostAsJsonAsync("/api/videolikes", new { VideoId = video.Id, IsLiked = false });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        Assert.Equal(0, json.GetProperty("likeCount").GetInt32());
        Assert.Equal(1, json.GetProperty("dislikeCount").GetInt32());
    }

    [Fact]
    public async Task Like_SameVoteAgain_NoChange()
    {
        var userId = Guid.NewGuid();
        var video = await SeedVideo();
        SetAuth(userId);

        await _client.PostAsJsonAsync("/api/videolikes", new { VideoId = video.Id, IsLiked = true });
        var response = await _client.PostAsJsonAsync("/api/videolikes", new { VideoId = video.Id, IsLiked = true });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        Assert.Equal(1, json.GetProperty("likeCount").GetInt32());
    }
}
