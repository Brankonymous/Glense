using System;

namespace Glense.VideoCatalogue.DTOs;

public class UploadResponseDTO
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string VideoUrl { get; set; } = null!;
    public string? ThumbnailUrl { get; set; }
    public DateTime UploadDate { get; set; }
    public int UploaderId { get; set; }
    public int ViewCount { get; set; }
    public int LikeCount { get; set; }
    public int DislikeCount { get; set; }
}
