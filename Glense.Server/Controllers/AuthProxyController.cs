using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace Glense.Server.Controllers;

/// <summary>
/// Proxy controller for Account Service authentication endpoints
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthProxyController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AuthProxyController> _logger;

    public AuthProxyController(IHttpClientFactory httpClientFactory, ILogger<AuthProxyController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login()
    {
        return await ProxyToAccountService("/api/auth/login");
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register()
    {
        return await ProxyToAccountService("/api/auth/register");
    }

    private async Task<IActionResult> ProxyToAccountService(string path)
    {
        var client = _httpClientFactory.CreateClient("AccountService");

        try
        {
            // Read request body
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            // Create request to Account Service
            var request = new HttpRequestMessage(HttpMethod.Post, path)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };

            // Forward authorization header if present
            if (Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                request.Headers.Add("Authorization", authHeader.ToString());
            }

            var response = await client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Proxied {Path} to Account Service, status: {Status}", path, response.StatusCode);

            return new ContentResult
            {
                StatusCode = (int)response.StatusCode,
                Content = content,
                ContentType = "application/json"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error proxying to Account Service: {Path}", path);
            return StatusCode(502, new { message = "Account service unavailable" });
        }
    }
}
