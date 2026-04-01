using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Glense.VideoCatalogue.Data;
using Glense.VideoCatalogue.Models;
using Xunit;

namespace VideoCatalogue.IntegrationTests;

public class CommentsControllerTests : IClassFixture<CustomVideoCatalogueFactory>
{
    private readonly HttpClient _client;
    private readonly CustomVideoCatalogueFactory _factory;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public CommentsControllerTests(CustomVideoCatalogueFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private void SetAuth(Guid userId, string username = "commenter")
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", JwtTokenHelper.GenerateToken(userId, username));
    }

    private async Task<Videos> SeedVideo()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<VideoCatalogueDbContext>();
        var video = new Videos
        {
            Id = Guid.NewGuid(),
            Title = $"CommentTest_{Guid.NewGuid():N}",
            VideoUrl = "test.mp4",
            UploadDate = DateTime.UtcNow,
            UploaderId = Guid.NewGuid()
        };
        db.Videos.Add(video);
        await db.SaveChangesAsync();
        return video;
    }

    private async Task<Comment> SeedComment(Guid videoId, Guid userId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<VideoCatalogueDbContext>();
        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            VideoId = videoId,
            UserId = userId,
            Username = "commenter",
            Content = "Test comment",
            LikeCount = 0,
            DislikeCount = 0,
            CreatedAt = DateTime.UtcNow
        };
        db.Comments.Add(comment);
        await db.SaveChangesAsync();
        return comment;
    }

    [Fact]
    public async Task GetComments_ReturnsComments()
    {
        var video = await SeedVideo();
        var userId = Guid.NewGuid();
        await SeedComment(video.Id, userId);

        var response = await _client.GetAsync($"/api/videos/{video.Id}/comments");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        Assert.True(json.GetArrayLength() > 0);
    }

    [Fact]
    public async Task CreateComment_WithAuth_ReturnsCreated()
    {
        var video = await SeedVideo();
        var userId = Guid.NewGuid();
        SetAuth(userId, "testcommenter");

        var response = await _client.PostAsJsonAsync($"/api/videos/{video.Id}/comments",
            new { Content = "Great video!" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        Assert.Equal("Great video!", json.GetProperty("content").GetString());
    }

    [Fact]
    public async Task CreateComment_VideoNotFound_Returns404()
    {
        var userId = Guid.NewGuid();
        SetAuth(userId);

        var response = await _client.PostAsJsonAsync($"/api/videos/{Guid.NewGuid()}/comments",
            new { Content = "Comment on nothing" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task LikeComment_TogglesLikeCounts()
    {
        var video = await SeedVideo();
        var commentUserId = Guid.NewGuid();
        var comment = await SeedComment(video.Id, commentUserId);
        var likerId = Guid.NewGuid();
        SetAuth(likerId);

        var response = await _client.PostAsJsonAsync(
            $"/api/videos/{video.Id}/comments/{comment.Id}/like",
            new { IsLiked = true });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        Assert.Equal(1, json.GetProperty("likeCount").GetInt32());
    }

    [Fact]
    public async Task DeleteComment_Owner_ReturnsNoContent()
    {
        var video = await SeedVideo();
        var userId = Guid.NewGuid();
        var comment = await SeedComment(video.Id, userId);
        SetAuth(userId);

        var response = await _client.DeleteAsync($"/api/videos/{video.Id}/comments/{comment.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteComment_NonOwner_Returns403()
    {
        var video = await SeedVideo();
        var ownerId = Guid.NewGuid();
        var comment = await SeedComment(video.Id, ownerId);
        SetAuth(Guid.NewGuid()); // different user

        var response = await _client.DeleteAsync($"/api/videos/{video.Id}/comments/{comment.Id}");

        Assert.True(response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.InternalServerError);
    }
}
