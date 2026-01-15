using System;
using System.ComponentModel.DataAnnotations;

namespace Glense.VideoCatalogue.DTOs;

public class LikeResponseDTO
{
    [Required]
    public Guid VideoId { get; set; }
    [Required]
    public bool IsLiked { get; set; }
    public int LikeCount { get; set; }
    public int DislikeCount { get; set; }
}
