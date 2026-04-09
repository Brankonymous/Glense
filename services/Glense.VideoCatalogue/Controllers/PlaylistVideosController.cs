using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Glense.VideoCatalogue.Data;
using Glense.VideoCatalogue.Models;
using Microsoft.EntityFrameworkCore;

namespace Glense.VideoCatalogue.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlaylistVideosController : ControllerBase
{
    private readonly VideoCatalogueDbContext _db;

    public PlaylistVideosController(VideoCatalogueDbContext db)
    {
        _db = db;
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] DTOs.AddPlaylistVideoRequestDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var playlist = await _db.Playlists.FindAsync(dto.PlaylistId);
        if (playlist == null) return NotFound("Playlist not found");
        if (playlist.CreatorId != GetCurrentUserId()) return Forbid();

        var exists = await _db.PlaylistVideos.AnyAsync(pv => pv.PlaylistId == dto.PlaylistId && pv.VideoId == dto.VideoId);
        if (exists) return Conflict("Video already in playlist");

        var pv = new PlaylistVideos { PlaylistId = dto.PlaylistId, VideoId = dto.VideoId };
        _db.PlaylistVideos.Add(pv);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [Authorize]
    [HttpDelete]
    public async Task<IActionResult> Remove([FromBody] DTOs.AddPlaylistVideoRequestDTO dto)
    {
        var playlist = await _db.Playlists.FindAsync(dto.PlaylistId);
        if (playlist == null) return NotFound("Playlist not found");
        if (playlist.CreatorId != GetCurrentUserId()) return Forbid();

        var pv = await _db.PlaylistVideos.FirstOrDefaultAsync(p => p.PlaylistId == dto.PlaylistId && p.VideoId == dto.VideoId);
        if (pv == null) return NotFound();
        _db.PlaylistVideos.Remove(pv);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("{playlistId:guid}")]
    public async Task<IActionResult> ListVideos(System.Guid playlistId)
    {
        var videos = await _db.PlaylistVideos
            .Where(pv => pv.PlaylistId == playlistId)
            .Include(pv => pv.Video)
            .Select(pv => new DTOs.UploadResponseDTO
            {
                Id = pv.Video.Id,
                Title = pv.Video.Title,
                Description = pv.Video.Description,
                VideoUrl = pv.Video.VideoUrl,
                ThumbnailUrl = pv.Video.ThumbnailUrl,
                UploadDate = pv.Video.UploadDate,
                UploaderId = pv.Video.UploaderId,
                ViewCount = pv.Video.ViewCount,
                LikeCount = pv.Video.LikeCount,
                DislikeCount = pv.Video.DislikeCount
            })
            .ToListAsync();

        return Ok(videos);
    }
}
