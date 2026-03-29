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
    public class VideoLikesController : ControllerBase
    {
        private readonly VideoCatalogueDbContext _db;

        public VideoLikesController(VideoCatalogueDbContext db)
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
        public async Task<IActionResult> Like([FromBody] DTOs.LikeRequestDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            var video = await _db.Videos.FirstOrDefaultAsync(v => v.Id == dto.VideoId);
            if (video == null) return NotFound();

            var existing = await _db.VideoLikes.FirstOrDefaultAsync(l => l.UserId == userId && l.VideoId == dto.VideoId);

            if (existing == null)
            {
                // New vote
                _db.VideoLikes.Add(new VideoLikes { UserId = userId, VideoId = dto.VideoId, IsLiked = dto.IsLiked });
                if (dto.IsLiked) video.LikeCount++;
                else video.DislikeCount++;
            }
            else if (existing.IsLiked != dto.IsLiked)
            {
                // Switching vote
                existing.IsLiked = dto.IsLiked;
                if (dto.IsLiked) { video.LikeCount++; video.DislikeCount = Math.Max(0, video.DislikeCount - 1); }
                else { video.DislikeCount++; video.LikeCount = Math.Max(0, video.LikeCount - 1); }
            }
            // else: same vote again, no change

            await _db.SaveChangesAsync();

            var resp = new DTOs.LikeResponseDTO { VideoId = dto.VideoId, IsLiked = dto.IsLiked, LikeCount = video.LikeCount, DislikeCount = video.DislikeCount };
            return Ok(resp);
        }
    }
