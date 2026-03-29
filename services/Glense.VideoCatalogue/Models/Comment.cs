using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Glense.VideoCatalogue.Models;

[Table("Comments")]
public class Comment
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("video_id")]
    public Guid VideoId { get; set; }

    [Required]
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("username")]
    public string Username { get; set; } = null!;

    [Required]
    [MaxLength(2000)]
    [Column("content")]
    public string Content { get; set; } = null!;

    [Column("like_count")]
    public int LikeCount { get; set; }

    [Column("dislike_count")]
    public int DislikeCount { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    public Videos? Video { get; set; }
}
