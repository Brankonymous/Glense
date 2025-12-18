using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Glense.VideoCatalogue.Data;
using Glense.VideoCatalogue.Models;
using Microsoft.EntityFrameworkCore;

namespace Glense.VideoCatalogue.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VideoLikesController : ControllerBase
    {
        private readonly VideoCatalogueDbContext _db;

        public VideoLikesController(VideoCatalogueDbContext db)
        {
            _db = db;
        }

        [HttpPost]
        public async Task<IActionResult> Like([FromBody] DTOs.LikeRequestDTO dto, [FromHeader(Name = "X-User-Id")] int userId = 0)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var like = await _db.VideoLikes.FirstOrDefaultAsync(l => l.UserId == userId && l.VideoId == dto.VideoId);
            if (like == null)
            {
                like = new VideoLikes { UserId = userId, VideoId = dto.VideoId, IsLiked = dto.IsLiked };
                _db.VideoLikes.Add(like);
            }
            else
            {
                like.IsLiked = dto.IsLiked;
                _db.VideoLikes.Update(like);
            }

            // Update counts
            var video = await _db.Videos.FirstOrDefaultAsync(v => v.Id == dto.VideoId);
            if (video != null)
            {
                var likes = await _db.VideoLikes.CountAsync(vl => vl.VideoId == dto.VideoId && vl.IsLiked);
                var dislikes = await _db.VideoLikes.CountAsync(vl => vl.VideoId == dto.VideoId && !vl.IsLiked);
                video.LikeCount = likes;
                video.DislikeCount = dislikes;
                _db.Videos.Update(video);
            }

            await _db.SaveChangesAsync();

            var resp = new DTOs.LikeResponseDTO { VideoId = dto.VideoId, IsLiked = dto.IsLiked, LikeCount = video?.LikeCount ?? 0, DislikeCount = video?.DislikeCount ?? 0 };
            return Ok(resp);
        }
    }
}
