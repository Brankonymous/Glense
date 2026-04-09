using System.Security.Claims;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Glense.VideoCatalogue.Data;
using Glense.VideoCatalogue.GrpcClients;
using Glense.Shared.Messages;
using Glense.VideoCatalogue.Models;
using Microsoft.EntityFrameworkCore;

namespace Glense.VideoCatalogue.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubscriptionsController : ControllerBase
{
    private readonly VideoCatalogueDbContext _db;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IAccountGrpcClient _accountClient;
    private readonly ILogger<SubscriptionsController> _logger;

    public SubscriptionsController(
        VideoCatalogueDbContext db,
        IPublishEndpoint publishEndpoint,
        IAccountGrpcClient accountClient,
        ILogger<SubscriptionsController> logger)
    {
        _db = db;
        _publishEndpoint = publishEndpoint;
        _accountClient = accountClient;
        _logger = logger;
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Subscribe([FromBody] DTOs.SubscribeRequestDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var subscriberId = GetCurrentUserId();
        var exists = await _db.Subscriptions.AnyAsync(s => s.SubscriberId == subscriberId && s.SubscribedToId == dto.SubscribedToId);
        if (exists) return Conflict("Already subscribed");

        var s = new Subscriptions { SubscriberId = subscriberId, SubscribedToId = dto.SubscribedToId, SubscriptionDate = System.DateTime.UtcNow };
        _db.Subscriptions.Add(s);
        await _db.SaveChangesAsync();

        // Publish UserSubscribedEvent so Account Service creates the notification
        try
        {
            var subscriberUsername = await _accountClient.GetUsernameAsync(subscriberId) ?? "Someone";
            await _publishEndpoint.Publish(new UserSubscribedEvent
            {
                SubscriberId = subscriberId,
                ChannelOwnerId = dto.SubscribedToId,
                SubscriberUsername = subscriberUsername
            });
            _logger.LogInformation(
                "Published UserSubscribedEvent: SubscriberId={SubscriberId}, ChannelOwnerId={ChannelOwnerId}",
                subscriberId, dto.SubscribedToId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to publish UserSubscribedEvent for subscription {SubscriberId} -> {ChannelOwnerId}",
                subscriberId, dto.SubscribedToId);
        }

        var resp = new DTOs.SubscribeResponseDTO { SubscriberId = s.SubscriberId, SubscribedToId = s.SubscribedToId, SubscriptionDate = s.SubscriptionDate };
        return Created(string.Empty, resp);
    }

    [Authorize]
    [HttpDelete]
    public async Task<IActionResult> Unsubscribe([FromBody] DTOs.SubscribeRequestDTO dto)
    {
        var subscriberId = GetCurrentUserId();
        var s = await _db.Subscriptions.FirstOrDefaultAsync(x => x.SubscriberId == subscriberId && x.SubscribedToId == dto.SubscribedToId);
        if (s == null) return NotFound();
        _db.Subscriptions.Remove(s);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
