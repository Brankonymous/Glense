using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Glense.VideoCatalogue.Data;
using Glense.VideoCatalogue.GrpcClients;
using Glense.VideoCatalogue.Services;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Glense.VideoCatalogue.Controllers;
    [ApiController]
    [Route("api/[controller]")]
    public class VideosController : ControllerBase
    {
        private readonly Upload _uploader;
        private readonly VideoCatalogueDbContext _db;
        private readonly IVideoStorage _storage;
        private readonly IAccountGrpcClient _accountClient;
        private readonly ILogger<VideosController> _logger;

        public VideosController(
            Upload uploader,
            VideoCatalogueDbContext db,
            IVideoStorage storage,
            IAccountGrpcClient accountClient,
            ILogger<VideosController> logger)
        {
            _uploader = uploader;
            _db = db;
            _storage = storage;
            _accountClient = accountClient;
            _logger = logger;
        }

        private Guid GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
        }

        private static string? ResolveThumbnailUrl(Guid videoId, string? thumbnailUrl)
        {
            if (string.IsNullOrEmpty(thumbnailUrl)) return null;
            if (thumbnailUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return thumbnailUrl;
            return $"/api/videos/{videoId}/thumbnail";
        }

        [Authorize]
        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] DTOs.UploadRequestDTO dto)
        {
            if (dto == null || dto.File == null || dto.File.Length == 0) return BadRequest("No file provided");

            var uploaderId = GetCurrentUserId();
            var video = await _uploader.UploadFileAsync(dto.File, dto.Title, dto.Description, uploaderId, dto.Thumbnail, dto.Category);

            var resp = new DTOs.UploadResponseDTO
            {
                Id = video.Id,
                Title = video.Title,
                Description = video.Description,
                VideoUrl = video.VideoUrl,
                ThumbnailUrl = ResolveThumbnailUrl(video.Id, video.ThumbnailUrl),
                UploadDate = video.UploadDate,
                UploaderId = video.UploaderId,
                ViewCount = video.ViewCount,
                LikeCount = video.LikeCount,
                DislikeCount = video.DislikeCount,
                Category = video.Category
            };

            return CreatedAtAction(nameof(Get), new { id = resp.Id }, resp);
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var videos = await _db.Videos.ToListAsync();
            var uploaderIds = videos.Select(v => v.UploaderId).ToList();
            var usernames = await _accountClient.GetUsernamesAsync(uploaderIds);

            var vids = videos.Select(video => new DTOs.UploadResponseDTO
            {
                Id = video.Id,
                Title = video.Title,
                Description = video.Description,
                VideoUrl = video.VideoUrl,
                ThumbnailUrl = ResolveThumbnailUrl(video.Id, video.ThumbnailUrl),
                UploadDate = video.UploadDate,
                UploaderId = video.UploaderId,
                UploaderUsername = usernames.GetValueOrDefault(video.UploaderId),
                ViewCount = video.ViewCount,
                LikeCount = video.LikeCount,
                DislikeCount = video.DislikeCount,
                Category = video.Category
            }).ToList();

            return Ok(vids);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var video = await _db.Videos.FirstOrDefaultAsync(v => v.Id == id);
            if (video == null) return NotFound();

            var username = await _accountClient.GetUsernameAsync(video.UploaderId);

            var resp = new DTOs.UploadResponseDTO
            {
                Id = video.Id,
                Title = video.Title,
                Description = video.Description,
                VideoUrl = video.VideoUrl,
                ThumbnailUrl = ResolveThumbnailUrl(video.Id, video.ThumbnailUrl),
                UploadDate = video.UploadDate,
                UploaderId = video.UploaderId,
                UploaderUsername = username,
                ViewCount = video.ViewCount,
                LikeCount = video.LikeCount,
                DislikeCount = video.DislikeCount,
                Category = video.Category
            };

            return Ok(resp);
        }

        [Authorize]
        [HttpPatch("{id:guid}/category")]
        public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] DTOs.UpdateCategoryDTO dto)
        {
            var video = await _db.Videos.FirstOrDefaultAsync(v => v.Id == id);
            if (video == null) return NotFound();
            if (video.UploaderId != GetCurrentUserId()) return Forbid();

            video.Category = dto.Category;
            await _db.SaveChangesAsync();
            return Ok(new { category = video.Category });
        }

        [HttpGet("{id:guid}/thumbnail")]
        public async Task<IActionResult> Thumbnail(Guid id)
        {
            var video = await _db.Videos.FirstOrDefaultAsync(v => v.Id == id);
            if (video == null || string.IsNullOrEmpty(video.ThumbnailUrl)) return NotFound();

            var physicalPath = _storage.GetPhysicalPath(video.ThumbnailUrl);
            if (physicalPath == null) return NotFound();

            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(video.ThumbnailUrl, out var contentType))
            {
                contentType = "image/jpeg";
            }

            return PhysicalFile(physicalPath, contentType);
        }

        [HttpGet("{id:guid}/stream")]
        public async Task<IActionResult> Stream(Guid id)
        {
            var video = await _db.Videos.FirstOrDefaultAsync(v => v.Id == id);
            if (video == null) return NotFound();

            var storedName = video.VideoUrl;
            if (string.IsNullOrEmpty(storedName)) return NotFound();

            var physicalPath = _storage.GetPhysicalPath(storedName);
            if (physicalPath == null) return NotFound();

            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(storedName, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            return PhysicalFile(physicalPath, contentType, enableRangeProcessing: true);
        }
    }
