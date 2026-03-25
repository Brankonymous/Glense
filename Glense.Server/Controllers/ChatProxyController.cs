using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace Glense.Server.Controllers;

[ApiController]
[Route("api")]
public class ChatProxyController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ChatProxyController> _logger;

    public ChatProxyController(IHttpClientFactory httpClientFactory, ILogger<ChatProxyController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [HttpGet("chats")]
    public Task<IActionResult> GetChats([FromQuery] Guid? cursor, [FromQuery] int pageSize = 50)
    {
        var qs = $"?pageSize={pageSize}" + (cursor.HasValue ? $"&cursor={cursor}" : "");
        return ProxyGet($"/api/chats{qs}");
    }

    [HttpPost("chats")]
    public Task<IActionResult> CreateChat() => ProxyPost("/api/chats");

    [HttpGet("chats/{chatId:guid}")]
    public Task<IActionResult> GetChat(Guid chatId) => ProxyGet($"/api/chats/{chatId}");

    [HttpDelete("chats/{chatId:guid}")]
    public Task<IActionResult> DeleteChat(Guid chatId) => ProxyDelete($"/api/chats/{chatId}");

    [HttpGet("chats/{chatId:guid}/messages")]
    public Task<IActionResult> GetMessages(Guid chatId, [FromQuery] Guid? cursor, [FromQuery] int pageSize = 50)
    {
        var qs = $"?pageSize={pageSize}" + (cursor.HasValue ? $"&cursor={cursor}" : "");
        return ProxyGet($"/api/chats/{chatId}/messages{qs}");
    }

    [HttpPost("chats/{chatId:guid}/messages")]
    public Task<IActionResult> PostMessage(Guid chatId) => ProxyPost($"/api/chats/{chatId}/messages");

    [HttpGet("messages/{messageId:guid}")]
    public Task<IActionResult> GetMessage(Guid messageId) => ProxyGet($"/api/messages/{messageId}");

    [HttpDelete("messages/{messageId:guid}")]
    public Task<IActionResult> DeleteMessage(Guid messageId) => ProxyDelete($"/api/messages/{messageId}");

    private async Task<IActionResult> ProxyGet(string path)
    {
        var client = _httpClientFactory.CreateClient("ChatService");
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
        var client = _httpClientFactory.CreateClient("ChatService");
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
        var client = _httpClientFactory.CreateClient("ChatService");
        try
        {
            var req = new HttpRequestMessage(HttpMethod.Delete, path);
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
    }

    private static async Task<ContentResult> ToResult(HttpResponseMessage resp)
    {
        var content = await resp.Content.ReadAsStringAsync();
        return new ContentResult { StatusCode = (int)resp.StatusCode, Content = content, ContentType = "application/json" };
    }

    private IActionResult ServiceUnavailable(Exception ex, string path)
    {
        _logger.LogError(ex, "Error proxying to Chat Service: {Path}", path);
        return StatusCode(502, new { message = "Chat service unavailable" });
    }
}
