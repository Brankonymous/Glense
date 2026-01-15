using System;
using System.ComponentModel.DataAnnotations;

namespace Glense.VideoCatalogue.DTOs;

public class UploadResponseDTO
{
    [Required]
    public Guid Id { get; set; }
    [Required]
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    [Required]
    public string VideoUrl { get; set; } = null!;
    public string? ThumbnailUrl { get; set; }
    public DateTime UploadDate { get; set; }
    [Required]
    public int UploaderId { get; set; }
    public int ViewCount { get; set; }
    public int LikeCount { get; set; }
    public int DislikeCount { get; set; }
}
