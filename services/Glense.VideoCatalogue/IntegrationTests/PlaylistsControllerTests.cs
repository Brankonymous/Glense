using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Glense.VideoCatalogue.Data;
using Glense.VideoCatalogue.Models;
using Xunit;

namespace VideoCatalogue.IntegrationTests;

public class PlaylistsControllerTests : IClassFixture<CustomVideoCatalogueFactory>
{
    private readonly HttpClient _client;
    private readonly CustomVideoCatalogueFactory _factory;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public PlaylistsControllerTests(CustomVideoCatalogueFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private void SetAuth(Guid userId)
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", JwtTokenHelper.GenerateToken(userId));
    }

    private async Task<Playlists> SeedPlaylist(Guid creatorId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<VideoCatalogueDbContext>();
        var playlist = new Playlists
        {
            Id = Guid.NewGuid(),
            Name = $"Playlist_{Guid.NewGuid():N}",
            Description = "Test playlist",
            CreatorId = creatorId,
            CreationDate = DateTime.UtcNow
        };
        db.Playlists.Add(playlist);
        await db.SaveChangesAsync();
        return playlist;
    }

    [Fact]
    public async Task Create_ReturnsCreated()
    {
        var userId = Guid.NewGuid();
        SetAuth(userId);

        var response = await _client.PostAsJsonAsync("/api/playlists", new { Name = "My Playlist", Description = "desc" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        Assert.Equal("My Playlist", json.GetProperty("name").GetString());
        Assert.Equal(userId, json.GetProperty("creatorId").GetGuid());
    }

    [Fact]
    public async Task List_ReturnsAll()
    {
        var userId = Guid.NewGuid();
        await SeedPlaylist(userId);

        var response = await _client.GetAsync("/api/playlists");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        Assert.True(json.GetArrayLength() > 0);
    }

    [Fact]
    public async Task List_FilterByCreatorId()
    {
        var creatorId = Guid.NewGuid();
        await SeedPlaylist(creatorId);

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/playlists");
        request.Headers.Add("X-Creator-Id", creatorId.ToString());
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        foreach (var item in json.EnumerateArray())
        {
            Assert.Equal(creatorId, item.GetProperty("creatorId").GetGuid());
        }
    }

    [Fact]
    public async Task Get_ExistingPlaylist_Returns()
    {
        var playlist = await SeedPlaylist(Guid.NewGuid());

        var response = await _client.GetAsync($"/api/playlists/{playlist.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        Assert.Equal(playlist.Name, json.GetProperty("name").GetString());
    }

    [Fact]
    public async Task Get_NonExistent_Returns404()
    {
        var response = await _client.GetAsync($"/api/playlists/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
