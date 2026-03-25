using System;
using System.ComponentModel.DataAnnotations;

namespace Glense.VideoCatalogue.DTOs;

public class CommentResponseDTO
{
    public Guid Id { get; set; }
    public Guid VideoId { get; set; }
    public Guid UserId { get; set; }
    public string Username { get; set; } = null!;
    public string Content { get; set; } = null!;
    public int LikeCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateCommentRequestDTO
{
    [Required]
    [MaxLength(2000)]
    public string Content { get; set; } = null!;
}
