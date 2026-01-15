using System.ComponentModel.DataAnnotations;

namespace Glense.VideoCatalogue.DTOs;

public class CreatePlaylistRequestDTO
{
	[Required]
	[MaxLength(255)]
	public string Name { get; set; } = null!;

	public string? Description { get; set; }
}
