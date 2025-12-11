using Glense.AccountService.DTOs;

namespace Glense.AccountService.Services
{
    public interface INotificationService
    {
        Task<NotificationDto> CreateNotificationAsync(Guid userId, string title, string message, string type, Guid? relatedEntityId = null);
    
        Task<List<NotificationDto>> GetUserNotificationsAsync(Guid userId, bool? isRead = null, int skip = 0, int take = 20);

        Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId);

        Task<bool> MarkAllAsReadAsync(Guid userId);
        
        Task<int> GetUnreadCountAsync(Guid userId);
    }
}
