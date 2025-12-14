using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Glense.ChatService.Models;

public class Message
{
    [Key]
    public Guid Id { get; set; }

    [ForeignKey(nameof(Chat))]
    public Guid ChatId { get; set; }
    public Chat Chat { get; set; } = default!;

    public MessageSender Sender { get; set; }

    [Required]
    public string Content { get; set; } = default!;

    public DateTime CreatedAtUtc { get; set; }
}
