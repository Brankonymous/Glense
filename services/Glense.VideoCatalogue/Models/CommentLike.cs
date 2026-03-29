using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Glense.VideoCatalogue.Models;

[Table("CommentLikes")]
public class CommentLike
{
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("comment_id")]
    public Guid CommentId { get; set; }

    [Column("is_liked")]
    public bool IsLiked { get; set; }

    public Comment? Comment { get; set; }
}
