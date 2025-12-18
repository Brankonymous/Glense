using System;

namespace Glense.VideoCatalogue.DTOs;

public class CreatePlaylistResponseDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime CreationDate { get; set; }
    public int CreatorId { get; set; }
}
