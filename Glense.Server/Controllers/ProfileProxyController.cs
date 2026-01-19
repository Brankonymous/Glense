using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace Glense.Server.Controllers;

/// <summary>
/// Proxy controller for Account Service profile endpoints
/// </summary>
[ApiController]
[Route("api/profile")]
public class ProfileProxyController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ProfileProxyController> _logger;

    public ProfileProxyController(IHttpClientFactory httpClientFactory, ILogger<ProfileProxyController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentProfile()
    {
        return await ProxyGetToAccountService("/api/profile/me");
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile()
    {
        return await ProxyPostToAccountService("/api/profile/me", HttpMethod.Put);
    }

    private async Task<IActionResult> ProxyGetToAccountService(string path)
    {
        var client = _httpClientFactory.CreateClient("AccountService");

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, path);

            // Forward authorization header
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

    private async Task<IActionResult> ProxyPostToAccountService(string path, HttpMethod method)
    {
        var client = _httpClientFactory.CreateClient("AccountService");

        try
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            var request = new HttpRequestMessage(method, path);

            if (!string.IsNullOrEmpty(body))
            {
                request.Content = new StringContent(body, Encoding.UTF8, "application/json");
            }

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
            _logger.LogError(ex, "Error proxying {Method} to Account Service: {Path}", method, path);
            return StatusCode(502, new { message = "Account service unavailable" });
        }
    }
}
