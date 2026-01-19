using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace Glense.Server.Controllers;

/// <summary>
/// Proxy controller for Donation Service donation endpoints
/// </summary>
[ApiController]
[Route("api/donation")]
public class DonationProxyController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<DonationProxyController> _logger;

    public DonationProxyController(IHttpClientFactory httpClientFactory, ILogger<DonationProxyController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [HttpGet("donor/{userId}")]
    public async Task<IActionResult> GetDonationsByDonor(string userId)
    {
        return await ProxyGetToDonationService($"/api/donation/donor/{userId}");
    }

    [HttpGet("recipient/{userId}")]
    public async Task<IActionResult> GetDonationsByRecipient(string userId)
    {
        return await ProxyGetToDonationService($"/api/donation/recipient/{userId}");
    }

    [HttpPost]
    public async Task<IActionResult> CreateDonation()
    {
        return await ProxyPostToDonationService("/api/donation");
    }

    private async Task<IActionResult> ProxyGetToDonationService(string path)
    {
        var client = _httpClientFactory.CreateClient("DonationService");

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
            _logger.LogError(ex, "Error proxying GET to Donation Service: {Path}", path);
            return StatusCode(502, new { message = "Donation service unavailable" });
        }
    }

    private async Task<IActionResult> ProxyPostToDonationService(string path)
    {
        var client = _httpClientFactory.CreateClient("DonationService");

        try
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            var request = new HttpRequestMessage(HttpMethod.Post, path)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };

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
            _logger.LogError(ex, "Error proxying POST to Donation Service: {Path}", path);
            return StatusCode(502, new { message = "Donation service unavailable" });
        }
    }
}
