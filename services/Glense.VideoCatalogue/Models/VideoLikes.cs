using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Glense.VideoCatalogue.Models;

[Table("VideoLikes")]
public class VideoLikes
{
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("video_id")]
    public Guid VideoId { get; set; }

    [Column("is_liked")]
    public bool IsLiked { get; set; }

    public Videos? Video { get; set; }
}

