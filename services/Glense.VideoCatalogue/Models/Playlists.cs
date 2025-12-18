using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Glense.VideoCatalogue.Models;

[Table("Playlists")]
public class Playlists
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(255)]
    [Column("name")]
    public string Name { get; set; } = null!;

    [Column("description")]
    public string? Description { get; set; }

    [Column("creator_id")]
    public int CreatorId { get; set; }

    [Column("creation_date")]
    public DateTime CreationDate { get; set; }

    public ICollection<PlaylistVideos>? PlaylistVideos { get; set; }
}

