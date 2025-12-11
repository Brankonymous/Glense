using Microsoft.EntityFrameworkCore;
using Glense.AccountService.Data;
using Glense.AccountService.DTOs;
using Glense.AccountService.Models;

namespace Glense.AccountService.Services
{
    public class NotificationService : INotificationService
    {
        private readonly AccountDbContext _context;

        public NotificationService(AccountDbContext context)
        {
            _context = context;
        }

        public async Task<NotificationDto> CreateNotificationAsync(
            Guid userId,
            string title,
            string message,
            string type,
            Guid? relatedEntityId = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                RelatedEntityId = relatedEntityId
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return MapToDto(notification);
        }

        public async Task<List<NotificationDto>> GetUserNotificationsAsync(
            Guid userId,
            bool? isRead = null,
            int skip = 0,
            int take = 20)
        {
            var query = _context.Notifications
                .Where(n => n.UserId == userId);

            if (isRead.HasValue)
            {
                query = query.Where(n => n.IsRead == isRead.Value);
            }

            var notifications = await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            return notifications.Select(MapToDto).ToList();
        }

        public async Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification == null)
                return false;

            notification.IsRead = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkAllAsReadAsync(Guid userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        private static NotificationDto MapToDto(Notification notification)
        {
            return new NotificationDto
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                IsRead = notification.IsRead,
                RelatedEntityId = notification.RelatedEntityId,
                CreatedAt = notification.CreatedAt
            };
        }
    }
}
