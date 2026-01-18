using System;
using System.ComponentModel.DataAnnotations;

namespace Glense.VideoCatalogue.DTOs;

public class PlaylistResponseDTO
{
    [Required]
    public Guid Id { get; set; }
    [Required]
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime CreationDate { get; set; }
    [Required]
    public int CreatorId { get; set; }
}
