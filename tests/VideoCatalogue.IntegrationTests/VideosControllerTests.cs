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

public class VideosControllerTests : IClassFixture<CustomVideoCatalogueFactory>
{
    private readonly HttpClient _client;
    private readonly CustomVideoCatalogueFactory _factory;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public VideosControllerTests(CustomVideoCatalogueFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private void SetAuth(Guid userId, string username = "testuser")
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", JwtTokenHelper.GenerateToken(userId, username));
    }

    private async Task<Videos> SeedVideo(Guid? uploaderId = null, string title = "Test Video", string? category = null)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<VideoCatalogueDbContext>();
        var video = new Videos
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = "Test description",
            VideoUrl = "test-video.mp4",
            UploadDate = DateTime.UtcNow,
            UploaderId = uploaderId ?? Guid.NewGuid(),
            ViewCount = 0,
            LikeCount = 0,
            DislikeCount = 0,
            Category = category
        };
        db.Videos.Add(video);
        await db.SaveChangesAsync();
        return video;
    }

    [Fact]
    public async Task Upload_WithFile_Returns201()
    {
        var userId = Guid.NewGuid();
        SetAuth(userId);

        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 0x00, 0x01, 0x02 });
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("video/mp4");
        content.Add(fileContent, "File", "test.mp4");
        content.Add(new StringContent("My Video"), "Title");

        var response = await _client.PostAsync("/api/videos/upload", content);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        Assert.Equal("My Video", json.GetProperty("title").GetString());
        Assert.Equal(userId, json.GetProperty("uploaderId").GetGuid());
    }

    [Fact]
    public async Task Upload_NoFile_Returns400()
    {
        var userId = Guid.NewGuid();
        SetAuth(userId);

        using var content = new MultipartFormDataContent();
        content.Add(new StringContent("My Video"), "Title");

        var response = await _client.PostAsync("/api/videos/upload", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Upload_NoJwt_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 0x00 });
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("video/mp4");
        content.Add(fileContent, "File", "test.mp4");

        var response = await _client.PostAsync("/api/videos/upload", content);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task List_ReturnsAllVideos()
    {
        await SeedVideo(title: $"ListTest_{Guid.NewGuid():N}");

        var response = await _client.GetAsync("/api/videos");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        Assert.True(json.GetArrayLength() > 0);
    }

    [Fact]
    public async Task Get_ExistingVideo_ReturnsVideo()
    {
        var video = await SeedVideo();

        var response = await _client.GetAsync($"/api/videos/{video.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        Assert.Equal(video.Title, json.GetProperty("title").GetString());
    }

    [Fact]
    public async Task Get_NonExistent_Returns404()
    {
        var response = await _client.GetAsync($"/api/videos/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Search_FindsByTitle()
    {
        var uniqueTitle = $"Searchable_{Guid.NewGuid():N}";
        await SeedVideo(title: uniqueTitle);

        var response = await _client.GetAsync($"/api/videos/search?q={uniqueTitle}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        Assert.True(json.GetArrayLength() > 0);
    }

    [Fact]
    public async Task Search_EmptyQuery_ReturnsEmpty()
    {
        var response = await _client.GetAsync("/api/videos/search?q=");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        Assert.Equal(0, json.GetArrayLength());
    }

    [Fact]
    public async Task Search_FilterByCategory()
    {
        var uniqueTitle = $"CatSearch_{Guid.NewGuid():N}";
        await SeedVideo(title: uniqueTitle, category: "music");

        var response = await _client.GetAsync($"/api/videos/search?q={uniqueTitle}&category=music");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        Assert.True(json.GetArrayLength() > 0);
    }

    [Fact]
    public async Task UpdateCategory_Owner_Succeeds()
    {
        var userId = Guid.NewGuid();
        var video = await SeedVideo(uploaderId: userId);
        SetAuth(userId);

        var response = await _client.PatchAsJsonAsync($"/api/videos/{video.Id}/category", new { Category = "gaming" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        Assert.Equal("gaming", json.GetProperty("category").GetString());
    }

    [Fact]
    public async Task UpdateCategory_NonOwner_Returns403()
    {
        var video = await SeedVideo(uploaderId: Guid.NewGuid());
        SetAuth(Guid.NewGuid()); // different user

        var response = await _client.PatchAsJsonAsync($"/api/videos/{video.Id}/category", new { Category = "gaming" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
