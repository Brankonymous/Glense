using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Glense.VideoCatalogue.Models;

[Table("PlaylistVideos")]
public class PlaylistVideos
{
    [Column("playlist_id")]
    public Guid PlaylistId { get; set; }

    [Column("video_id")]
    public Guid VideoId { get; set; }

    public Playlists? Playlist { get; set; }
    public Videos? Video { get; set; }
}

