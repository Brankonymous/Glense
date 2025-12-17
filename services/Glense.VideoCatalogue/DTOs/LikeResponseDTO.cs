using System;

namespace Glense.VideoCatalogue.DTOs;

public class LikeResponseDTO
{
    public Guid VideoId { get; set; }
    public bool IsLiked { get; set; }
    public int LikeCount { get; set; }
    public int DislikeCount { get; set; }
}
