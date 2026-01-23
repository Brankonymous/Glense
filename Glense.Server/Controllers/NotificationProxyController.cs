using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace Glense.Server.Controllers;

/// <summary>
/// Proxy controller for Account Service notification endpoints
/// </summary>
[ApiController]
[Route("api/notification")]
public class NotificationProxyController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<NotificationProxyController> _logger;

    public NotificationProxyController(IHttpClientFactory httpClientFactory, ILogger<NotificationProxyController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetNotifications()
    {
        return await ProxyGetToAccountService("/api/notification");
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(string id)
    {
        return await ProxyPutToAccountService($"/api/notification/{id}/read");
    }

    private async Task<IActionResult> ProxyGetToAccountService(string path)
    {
        var client = _httpClientFactory.CreateClient("AccountService");

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, path);

            if (Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                request.Headers.Add("Authorization", authHeader.ToString());
            }

            var response = await client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            return new ContentResult
            {
                StatusCode = (int)response.StatusCode,
                Content = content,
                ContentType = "application/json"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error proxying GET to Account Service: {Path}", path);
            return StatusCode(502, new { message = "Account service unavailable" });
        }
    }

    private async Task<IActionResult> ProxyPutToAccountService(string path)
    {
        var client = _httpClientFactory.CreateClient("AccountService");

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Put, path);

            if (Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                request.Headers.Add("Authorization", authHeader.ToString());
            }

            var response = await client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            return new ContentResult
            {
                StatusCode = (int)response.StatusCode,
                Content = content,
                ContentType = "application/json"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error proxying PUT to Account Service: {Path}", path);
            return StatusCode(502, new { message = "Account service unavailable" });
        }
    }
}
