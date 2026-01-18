using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Glense.VideoCatalogue.DTOs;

public class UploadRequestDTO
{
	[Required]
	public IFormFile File { get; set; } = null!;

	[MaxLength(255)]
	public string? Title { get; set; }

	public string? Description { get; set; }
}
