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
    public class PlaylistsController : ControllerBase
    {
        private readonly VideoCatalogueDbContext _db;

        public PlaylistsController(VideoCatalogueDbContext db)
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
        public async Task<IActionResult> Create([FromBody] DTOs.CreatePlaylistRequestDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var creatorId = GetCurrentUserId();
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

        [HttpGet]
        public async Task<IActionResult> List([FromHeader(Name = "X-Creator-Id")] Guid creatorId = default)
        {
            var q = _db.Playlists.AsQueryable();
            if (creatorId != Guid.Empty) q = q.Where(p => p.CreatorId == creatorId);

            var list = await q.Select(playlist => new DTOs.PlaylistResponseDTO
            {
                Id = playlist.Id,
                Name = playlist.Name,
                Description = playlist.Description,
                CreationDate = playlist.CreationDate,
                CreatorId = playlist.CreatorId
            }).ToListAsync();

            return Ok(list);
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
