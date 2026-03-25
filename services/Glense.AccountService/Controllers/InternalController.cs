using Microsoft.AspNetCore.Mvc;
using Glense.AccountService.DTOs;
using Glense.AccountService.Services;

namespace Glense.AccountService.Controllers
{
    [ApiController]
    [Route("api/internal")]
    public class InternalController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<InternalController> _logger;

        public InternalController(INotificationService notificationService, ILogger<InternalController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        [HttpPost("notifications")]
        public async Task<ActionResult<NotificationDto>> CreateNotification(
            [FromBody] CreateNotificationRequest request)
        {
            var notification = await _notificationService.CreateNotificationAsync(
                request.UserId,
                request.Title,
                request.Message,
                request.Type,
                request.RelatedEntityId);

            return Ok(notification);
        }
    }
}
