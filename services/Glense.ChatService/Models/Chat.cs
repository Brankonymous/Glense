using System.ComponentModel.DataAnnotations;

namespace Glense.ChatService.Models;

public class Chat
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Topic { get; set; } = default!;

    public DateTime CreatedAtUtc { get; set; }

    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
