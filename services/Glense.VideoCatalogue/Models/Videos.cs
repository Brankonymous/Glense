using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Glense.VideoCatalogue.Models;

[Table("Videos")]
public class Videos
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("title")]
    public string Title { get; set; } = null!;

    [Column("description")]
    public string? Description { get; set; }

    [Required]
    [Column("upload_date")]
    public DateTime UploadDate { get; set; }

    [Required]
    [Column("uploader_id")]
    public int UploaderId { get; set; }

    [MaxLength(512)]
    [Column("thumbnail_url")]
    public string? ThumbnailUrl { get; set; }

    [Required]
    [MaxLength(512)]
    [Column("video_url")]
    public string VideoUrl { get; set; } = null!;

    [Column("view_count")]
    public int ViewCount { get; set; }

    [Column("like_count")]
    public int LikeCount { get; set; }

    [Column("dislike_count")]
    public int DislikeCount { get; set; }

    public ICollection<PlaylistVideos>? PlaylistVideos { get; set; }
    public ICollection<VideoLikes>? VideoLikes { get; set; }
}
