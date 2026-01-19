using System;
using System.ComponentModel.DataAnnotations;

namespace Glense.VideoCatalogue.DTOs;

public class LikeRequestDTO
{
	[Required]
	public Guid VideoId { get; set; }

	[Required]
	public bool IsLiked { get; set; }
}
