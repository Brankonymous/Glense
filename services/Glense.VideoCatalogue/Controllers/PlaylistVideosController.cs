using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Glense.VideoCatalogue.Data;
using Glense.VideoCatalogue.Models;
using Microsoft.EntityFrameworkCore;

namespace Glense.VideoCatalogue.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlaylistVideosController : ControllerBase
    {
        private readonly VideoCatalogueDbContext _db;

        public PlaylistVideosController(VideoCatalogueDbContext db)
        {
            _db = db;
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] DTOs.AddPlaylistVideoRequestDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var exists = await _db.PlaylistVideos.AnyAsync(pv => pv.PlaylistId == dto.PlaylistId && pv.VideoId == dto.VideoId);
            if (exists) return Conflict("Video already in playlist");

            var pv = new PlaylistVideos { PlaylistId = dto.PlaylistId, VideoId = dto.VideoId };
            _db.PlaylistVideos.Add(pv);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> Remove([FromBody] DTOs.AddPlaylistVideoRequestDTO dto)
        {
            var pv = await _db.PlaylistVideos.FirstOrDefaultAsync(p => p.PlaylistId == dto.PlaylistId && p.VideoId == dto.VideoId);
            if (pv == null) return NotFound();
            _db.PlaylistVideos.Remove(pv);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
