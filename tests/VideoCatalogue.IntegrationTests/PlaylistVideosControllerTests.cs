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

public class PlaylistVideosControllerTests : IClassFixture<CustomVideoCatalogueFactory>
{
    private readonly HttpClient _client;
    private readonly CustomVideoCatalogueFactory _factory;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public PlaylistVideosControllerTests(CustomVideoCatalogueFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private void SetAuth(Guid userId)
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", JwtTokenHelper.GenerateToken(userId));
    }

    private async Task<(Playlists Playlist, Videos Video)> SeedPlaylistAndVideo(Guid ownerId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<VideoCatalogueDbContext>();

        var playlist = new Playlists
        {
            Id = Guid.NewGuid(),
            Name = $"PVTest_{Guid.NewGuid():N}",
            CreatorId = ownerId,
            CreationDate = DateTime.UtcNow
        };
        db.Playlists.Add(playlist);

        var video = new Videos
        {
            Id = Guid.NewGuid(),
            Title = $"PVVideo_{Guid.NewGuid():N}",
            VideoUrl = "test.mp4",
            UploadDate = DateTime.UtcNow,
            UploaderId = Guid.NewGuid()
        };
        db.Videos.Add(video);

        await db.SaveChangesAsync();
        return (playlist, video);
    }

    [Fact]
    public async Task Add_OwnerAddsVideo_ReturnsNoContent()
    {
        var ownerId = Guid.NewGuid();
        var (playlist, video) = await SeedPlaylistAndVideo(ownerId);
        SetAuth(ownerId);

        var response = await _client.PostAsJsonAsync("/api/playlistvideos",
            new { PlaylistId = playlist.Id, VideoId = video.Id });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Add_NonOwner_Returns403()
    {
        var ownerId = Guid.NewGuid();
        var (playlist, video) = await SeedPlaylistAndVideo(ownerId);
        SetAuth(Guid.NewGuid()); // different user

        var response = await _client.PostAsJsonAsync("/api/playlistvideos",
            new { PlaylistId = playlist.Id, VideoId = video.Id });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Add_AlreadyInPlaylist_Returns409()
    {
        var ownerId = Guid.NewGuid();
        var (playlist, video) = await SeedPlaylistAndVideo(ownerId);
        SetAuth(ownerId);

        await _client.PostAsJsonAsync("/api/playlistvideos",
            new { PlaylistId = playlist.Id, VideoId = video.Id });
        var response = await _client.PostAsJsonAsync("/api/playlistvideos",
            new { PlaylistId = playlist.Id, VideoId = video.Id });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Remove_OwnerRemovesVideo_ReturnsNoContent()
    {
        var ownerId = Guid.NewGuid();
        var (playlist, video) = await SeedPlaylistAndVideo(ownerId);
        SetAuth(ownerId);

        await _client.PostAsJsonAsync("/api/playlistvideos",
            new { PlaylistId = playlist.Id, VideoId = video.Id });

        var request = new HttpRequestMessage(HttpMethod.Delete, "/api/playlistvideos")
        {
            Content = JsonContent.Create(new { PlaylistId = playlist.Id, VideoId = video.Id })
        };
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Remove_NonOwner_Returns403()
    {
        var ownerId = Guid.NewGuid();
        var (playlist, video) = await SeedPlaylistAndVideo(ownerId);
        SetAuth(ownerId);
        await _client.PostAsJsonAsync("/api/playlistvideos",
            new { PlaylistId = playlist.Id, VideoId = video.Id });

        SetAuth(Guid.NewGuid()); // different user
        var request = new HttpRequestMessage(HttpMethod.Delete, "/api/playlistvideos")
        {
            Content = JsonContent.Create(new { PlaylistId = playlist.Id, VideoId = video.Id })
        };
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ListVideos_ReturnsVideosInPlaylist()
    {
        var ownerId = Guid.NewGuid();
        var (playlist, video) = await SeedPlaylistAndVideo(ownerId);
        SetAuth(ownerId);

        await _client.PostAsJsonAsync("/api/playlistvideos",
            new { PlaylistId = playlist.Id, VideoId = video.Id });

        var response = await _client.GetAsync($"/api/playlistvideos/{playlist.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        Assert.True(json.GetArrayLength() > 0);
    }
}
