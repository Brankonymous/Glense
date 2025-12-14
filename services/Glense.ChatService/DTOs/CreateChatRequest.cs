using System.ComponentModel.DataAnnotations;

namespace Glense.ChatService.DTOs;

public class CreateChatRequest
{
    [Required]
    [MaxLength(200)]
    public string Topic { get; set; } = default!;
}
