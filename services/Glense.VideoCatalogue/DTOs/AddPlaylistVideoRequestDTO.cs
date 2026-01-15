using System;
using System.ComponentModel.DataAnnotations;

namespace Glense.VideoCatalogue.DTOs;

public class AddPlaylistVideoRequestDTO
{
    [Required]
    public Guid PlaylistId { get; set; }

    [Required]
    public Guid VideoId { get; set; }
}
