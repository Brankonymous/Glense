using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace Glense.Server.Controllers;

[ApiController]
[Route("api")]
public class VideoProxyController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<VideoProxyController> _logger;

    public VideoProxyController(IHttpClientFactory httpClientFactory, ILogger<VideoProxyController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [HttpGet("videos")]
    public Task<IActionResult> ListVideos() => ProxyGet("/api/videos");

    [HttpGet("videos/{id:guid}")]
    public Task<IActionResult> GetVideo(Guid id) => ProxyGet($"/api/videos/{id}");

    [HttpGet("videos/{id:guid}/comments")]
    public Task<IActionResult> GetComments(Guid id) => ProxyGet($"/api/videos/{id}/comments");

    [HttpPost("videos/{id:guid}/comments")]
    public Task<IActionResult> PostComment(Guid id) => ProxyPost($"/api/videos/{id}/comments");

    [HttpDelete("videos/{videoId:guid}/comments/{commentId:guid}")]
    public Task<IActionResult> DeleteComment(Guid videoId, Guid commentId) => ProxyDelete($"/api/videos/{videoId}/comments/{commentId}");

    [HttpPost("videos/upload")]
    public Task<IActionResult> UploadVideo() => ProxyUpload("/api/videos/upload");

    [HttpGet("videos/{id:guid}/stream")]
    public Task<IActionResult> StreamVideo(Guid id) => ProxyGet($"/api/videos/{id}/stream");

    [HttpPost("subscriptions")]
    public Task<IActionResult> Subscribe() => ProxyPost("/api/subscriptions");

    [HttpDelete("subscriptions")]
    public Task<IActionResult> Unsubscribe() => ProxyDelete("/api/subscriptions");

    [HttpPost("videolikes")]
    public Task<IActionResult> Like() => ProxyPost("/api/videolikes");

    [HttpPost("playlists")]
    public Task<IActionResult> CreatePlaylist() => ProxyPost("/api/playlists");

    [HttpGet("playlists")]
    public Task<IActionResult> ListPlaylists() => ProxyGet("/api/playlists");

    [HttpGet("playlists/{id:guid}")]
    public Task<IActionResult> GetPlaylist(Guid id) => ProxyGet($"/api/playlists/{id}");

    [HttpPost("playlistvideos")]
    public Task<IActionResult> AddToPlaylist() => ProxyPost("/api/playlistvideos");

    [HttpDelete("playlistvideos")]
    public Task<IActionResult> RemoveFromPlaylist() => ProxyDelete("/api/playlistvideos");

    [HttpGet("playlistvideos/{playlistId:guid}")]
    public Task<IActionResult> ListPlaylistVideos(Guid playlistId) => ProxyGet($"/api/playlistvideos/{playlistId}");

    private async Task<IActionResult> ProxyGet(string path)
    {
        var client = _httpClientFactory.CreateClient("VideoService");
        try
        {
            var req = new HttpRequestMessage(HttpMethod.Get, path);
            ForwardHeaders(req);
            var resp = await client.SendAsync(req);
            return await ToResult(resp);
        }
        catch (Exception ex) { return ServiceUnavailable(ex, path); }
    }

    private async Task<IActionResult> ProxyPost(string path)
    {
        var client = _httpClientFactory.CreateClient("VideoService");
        try
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();
            var req = new HttpRequestMessage(HttpMethod.Post, path)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };
            ForwardHeaders(req);
            var resp = await client.SendAsync(req);
            return await ToResult(resp);
        }
        catch (Exception ex) { return ServiceUnavailable(ex, path); }
    }

    private async Task<IActionResult> ProxyDelete(string path)
    {
        var client = _httpClientFactory.CreateClient("VideoService");
        try
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();
            var req = new HttpRequestMessage(HttpMethod.Delete, path);
            if (!string.IsNullOrEmpty(body))
                req.Content = new StringContent(body, Encoding.UTF8, "application/json");
            ForwardHeaders(req);
            var resp = await client.SendAsync(req);
            return await ToResult(resp);
        }
        catch (Exception ex) { return ServiceUnavailable(ex, path); }
    }

    private async Task<IActionResult> ProxyUpload(string path)
    {
        var client = _httpClientFactory.CreateClient("VideoService");
        try
        {
            var content = new StreamContent(Request.Body);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(Request.ContentType ?? "application/octet-stream");
            var req = new HttpRequestMessage(HttpMethod.Post, path) { Content = content };
            ForwardHeaders(req);
            var resp = await client.SendAsync(req);
            return await ToResult(resp);
        }
        catch (Exception ex) { return ServiceUnavailable(ex, path); }
    }

    private void ForwardHeaders(HttpRequestMessage req)
    {
        if (Request.Headers.TryGetValue("Authorization", out var auth))
            req.Headers.Add("Authorization", auth.ToString());
        if (Request.Headers.TryGetValue("X-Uploader-Id", out var uid))
            req.Headers.Add("X-Uploader-Id", uid.ToString());
        if (Request.Headers.TryGetValue("X-User-Id", out var userId))
            req.Headers.Add("X-User-Id", userId.ToString());
        if (Request.Headers.TryGetValue("X-Creator-Id", out var cid))
            req.Headers.Add("X-Creator-Id", cid.ToString());
        if (Request.Headers.TryGetValue("X-Username", out var uname))
            req.Headers.Add("X-Username", uname.ToString());
    }

    private static async Task<ContentResult> ToResult(HttpResponseMessage resp)
    {
        var content = await resp.Content.ReadAsStringAsync();
        return new ContentResult { StatusCode = (int)resp.StatusCode, Content = content, ContentType = "application/json" };
    }

    private IActionResult ServiceUnavailable(Exception ex, string path)
    {
        _logger.LogError(ex, "Error proxying to Video Service: {Path}", path);
        return StatusCode(502, new { message = "Video service unavailable" });
    }
}
