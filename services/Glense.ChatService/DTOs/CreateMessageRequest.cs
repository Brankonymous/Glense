using System.ComponentModel.DataAnnotations;

namespace Glense.ChatService.DTOs;

public class CreateMessageRequest
{
    [Required]
    [RegularExpression("^(user|system)$", ErrorMessage = "sender must be 'user' or 'system'")]
    public string Sender { get; set; } = "user";

    [Required]
    [MaxLength(4000)]
    public string Content { get; set; } = default!;
}
