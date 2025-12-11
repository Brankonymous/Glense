using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Glense.AccountService.Models
{
    [Table("notifications")] 
    public class Notification
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [Column("user_id")]
        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        /// <summary>
        /// Short title/heading for the notification.
        /// Example: "New Subscriber", "Donation Received"
        /// </summary>
        [Required]
        [MaxLength(100)]
        [Column("title")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Detailed message body of the notification.
        /// Example: "John Doe subscribed to your channel"
        /// </summary>
        [Required]
        [Column("message")]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Category/type of the notification.
        /// Possible values:
        /// - "subscription": New subscriber notification
        /// - "donation": Donation received
        /// - "comment": New comment on your video
        /// - "system": System/administrative notifications
        /// </summary>
        [Required]
        [MaxLength(50)]
        [Column("type")]
        public string Type { get; set; } = string.Empty;

        [Column("is_read")]
        public bool IsRead { get; set; } = false; // Default to unread

        /// <summary>
        /// Optional ID of the related entity that triggered this notification.
        /// Examples:
        /// - For subscription: the subscriber's user ID
        /// - For donation: the donation ID
        /// - For comment: the comment ID
        /// </summary>
        [Column("related_entity_id")]
        public Guid? RelatedEntityId { get; set; } // Nullable because not all notifications need this

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
