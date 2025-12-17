using System;
using System.ComponentModel.DataAnnotations;

namespace Glense.VideoCatalogue.DTOs;

public class SubscribeRequestDTO
{
	[Required]
	public int SubscribedToId { get; set; }
}
