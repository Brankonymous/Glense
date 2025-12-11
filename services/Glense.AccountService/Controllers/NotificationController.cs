using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Glense.AccountService.DTOs;
using Glense.AccountService.Services;

namespace Glense.AccountService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(INotificationService notificationService, ILogger<NotificationController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim!);
        }

        [HttpGet]
        public async Task<ActionResult<List<NotificationDto>>> GetNotifications(
            [FromQuery] bool? isRead = null,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 20)
        {
            try
            {
                var userId = GetCurrentUserId();
                var notifications = await _notificationService.GetUserNotificationsAsync(userId, isRead, skip, take);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        [HttpGet("unread-count")]
        public async Task<ActionResult<int>> GetUnreadCount()
        {
            try
            {
                var userId = GetCurrentUserId();
                var count = await _notificationService.GetUnreadCountAsync(userId);
                return Ok(new { count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread count");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        [HttpPut("{notificationId}/read")]
        public async Task<IActionResult> MarkAsRead(Guid notificationId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var success = await _notificationService.MarkAsReadAsync(notificationId, userId);

                if (!success)
                {
                    return NotFound(new { message = "Notification not found" });
                }

                return Ok(new { message = "Notification marked as read" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                var userId = GetCurrentUserId();
                await _notificationService.MarkAllAsReadAsync(userId);
                return Ok(new { message = "All notifications marked as read" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all as read");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }
    }
}
