using System;
using System.IO;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Glense.VideoCatalogue.Data;
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
        private readonly HttpClient _accountClient;
        private readonly ILogger<VideosController> _logger;

        private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

        public VideosController(Upload uploader, VideoCatalogueDbContext db, IVideoStorage storage, IHttpClientFactory httpClientFactory, ILogger<VideosController> logger)
        {
            _uploader = uploader;
            _db = db;
            _storage = storage;
            _accountClient = httpClientFactory.CreateClient("AccountService");
            _logger = logger;
        }

        private static string? ResolveThumbnailUrl(Guid videoId, string? thumbnailUrl)
        {
            if (string.IsNullOrEmpty(thumbnailUrl)) return null;
            if (thumbnailUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return thumbnailUrl;
            return $"/api/videos/{videoId}/thumbnail";
        }

        private async Task<string?> GetUsernameAsync(Guid userId)
        {
            if (userId == Guid.Empty) return null;
            try
            {
                var resp = await _accountClient.GetAsync($"/api/profile/{userId}");
                if (!resp.IsSuccessStatusCode) return null;
                var json = await resp.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
                return json.TryGetProperty("username", out var u) ? u.GetString() : null;
            }
            catch { return null; }
        }

        private async Task<Dictionary<Guid, string>> ResolveUsernamesAsync(IEnumerable<Guid> userIds)
        {
            var map = new Dictionary<Guid, string>();
            var unique = userIds.Where(id => id != Guid.Empty).Distinct().ToList();
            var tasks = unique.Select(async id =>
            {
                var name = await GetUsernameAsync(id);
                if (name != null) map[id] = name;
            });
            await Task.WhenAll(tasks);
            return map;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] DTOs.UploadRequestDTO dto, [FromHeader(Name = "X-Uploader-Id")] Guid uploaderId = default)
        {
            if (dto == null || dto.File == null || dto.File.Length == 0) return BadRequest("No file provided");

            var video = await _uploader.UploadFileAsync(dto.File, dto.Title, dto.Description, uploaderId, dto.Thumbnail);

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
                DislikeCount = video.DislikeCount
            };

            return CreatedAtAction(nameof(Get), new { id = resp.Id }, resp);
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var videos = await _db.Videos.ToListAsync();
            var usernames = await ResolveUsernamesAsync(videos.Select(v => v.UploaderId));

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
                DislikeCount = video.DislikeCount
            }).ToList();

            return Ok(vids);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var video = await _db.Videos.FirstOrDefaultAsync(v => v.Id == id);
            if (video == null) return NotFound();

            var username = await GetUsernameAsync(video.UploaderId);

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
                DislikeCount = video.DislikeCount
            };

            return Ok(resp);
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
