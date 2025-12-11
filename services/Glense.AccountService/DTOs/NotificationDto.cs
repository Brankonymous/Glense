namespace Glense.AccountService.DTOs
{
    public class NotificationDto
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public bool IsRead { get; set; }


        // Examples:
        // - Type="subscription", RelatedEntityId = subscriber's user ID
        // - Type="donation", RelatedEntityId = donation ID
        // - Type="comment", RelatedEntityId = comment ID
        // - Type="system", RelatedEntityId = null (no related entity)
        //
        // Frontend can use this to create a link:
        // onClick={() => navigate(`/donation/${notification.relatedEntityId}`)}
        public Guid? RelatedEntityId { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
