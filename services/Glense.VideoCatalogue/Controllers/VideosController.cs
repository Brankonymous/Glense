using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Glense.VideoCatalogue.Data;
using Glense.VideoCatalogue.Services;
using Microsoft.EntityFrameworkCore;

namespace Glense.VideoCatalogue.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VideosController : ControllerBase
    {
        private readonly Upload _uploader;
        private readonly VideoCatalogueDbContext _db;
        private readonly IVideoStorage _storage;

        public VideosController(Upload uploader, VideoCatalogueDbContext db, IVideoStorage storage)
        {
            _uploader = uploader;
            _db = db;
            _storage = storage;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] DTOs.UploadRequestDTO dto, [FromHeader(Name = "X-Uploader-Id")] int uploaderId = 0)
        {
            if (dto == null || dto.File == null || dto.File.Length == 0) return BadRequest("No file provided");

            var video = await _uploader.UploadFileAsync(dto.File, dto.Title, dto.Description, uploaderId);

            var resp = new DTOs.UploadResponseDTO
            {
                Id = video.Id,
                Title = video.Title,
                Description = video.Description,
                VideoUrl = video.VideoUrl,
                ThumbnailUrl = video.ThumbnailUrl,
                UploadDate = video.UploadDate,
                UploaderId = video.UploaderId,
                ViewCount = video.ViewCount,
                LikeCount = video.LikeCount,
                DislikeCount = video.DislikeCount
            };

            return CreatedAtAction(nameof(Get), new { id = resp.Id }, resp);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var video = await _db.Videos.FirstOrDefaultAsync(v => v.Id == id);
            if (video == null) return NotFound();

            var resp = new DTOs.VideoResponseDTO
            {
                Id = video.Id,
                Title = video.Title,
                Description = video.Description,
                VideoUrl = video.VideoUrl,
                ThumbnailUrl = video.ThumbnailUrl,
                UploadDate = video.UploadDate,
                UploaderId = video.UploaderId,
                ViewCount = video.ViewCount,
                LikeCount = video.LikeCount,
                DislikeCount = video.DislikeCount
            };

            return Ok(resp);
        }

        [HttpGet("{id:guid}/stream")]
        public async Task<IActionResult> Stream(Guid id)
        {
            var video = await _db.Videos.FirstOrDefaultAsync(v => v.Id == id);
            if (video == null) return NotFound();

            var storedName = video.VideoUrl;
            if (string.IsNullOrEmpty(storedName)) return NotFound();

            Request.Headers.TryGetValue("Range", out var rangeHeader);
            long? start = null, end = null;
            if (!string.IsNullOrEmpty(rangeHeader))
            {
                // Expecting "bytes=start-end"
                var range = rangeHeader.ToString();
                if (range.StartsWith("bytes=", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = range.Substring(6).Split('-');
                    if (long.TryParse(parts[0], out var s)) start = s;
                    if (parts.Length > 1 && long.TryParse(parts[1], out var e)) end = e;
                }
            }

            var (stream, total) = await _storage.OpenReadRangeAsync(storedName, start, end);

            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(storedName, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            Response.Headers["Accept-Ranges"] = "bytes";

            if (start.HasValue)
            {
                var s = start.Value;
                var e = end ?? (total - 1);
                var contentLength = e - s + 1;
                Response.StatusCode = 206;
                Response.Headers["Content-Range"] = $"bytes {s}-{e}/{total}";
                Response.ContentLength = contentLength;
                return File(stream, contentType);
            }

            Response.ContentLength = total;
            return File(stream, contentType);
        }
    }
}
