using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Glense.VideoCatalogue.Data;
using Glense.VideoCatalogue.Models;
using Microsoft.EntityFrameworkCore;

namespace Glense.VideoCatalogue.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlaylistsController : ControllerBase
    {
        private readonly VideoCatalogueDbContext _db;

        public PlaylistsController(VideoCatalogueDbContext db)
        {
            _db = db;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DTOs.CreatePlaylistRequestDTO dto, [FromHeader(Name = "X-Creator-Id")] int creatorId = 0)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var playlist = new Playlists
            {
                Id = System.Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                CreatorId = creatorId,
                CreationDate = System.DateTime.UtcNow
            };

            _db.Playlists.Add(playlist);
            await _db.SaveChangesAsync();

            var resp = new DTOs.CreatePlaylistResponseDTO
            {
                Id = playlist.Id,
                Name = playlist.Name,
                Description = playlist.Description,
                CreationDate = playlist.CreationDate,
                CreatorId = playlist.CreatorId
            };

            return CreatedAtAction(nameof(Get), new { id = resp.Id }, resp);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(System.Guid id)
        {
            var playlist = await _db.Playlists.FirstOrDefaultAsync(p => p.Id == id);
            if (playlist == null) return NotFound();

            var resp = new DTOs.PlaylistResponseDTO
            {
                Id = playlist.Id,
                Name = playlist.Name,
                Description = playlist.Description,
                CreationDate = playlist.CreationDate,
                CreatorId = playlist.CreatorId
            };

            return Ok(resp);
        }
    }
}
