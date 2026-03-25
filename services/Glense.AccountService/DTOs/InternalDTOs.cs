namespace Glense.AccountService.DTOs
{
    public record CreateNotificationRequest(
        Guid UserId,
        string Title,
        string Message,
        string Type,
        Guid? RelatedEntityId = null);
}
