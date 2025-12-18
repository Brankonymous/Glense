using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Glense.VideoCatalogue.Data;
using Glense.VideoCatalogue.Models;

namespace Glense.VideoCatalogue.Services
{
	public class Upload
	{
		private readonly IVideoStorage _storage;
		private readonly VideoCatalogueDbContext _db;

		public Upload(IVideoStorage storage, VideoCatalogueDbContext db)
		{
			_storage = storage;
			_db = db;
		}

		public async Task<Videos> UploadFileAsync(IFormFile file, string? title, string? description, int uploaderId, CancellationToken cancellationToken = default)
		{
			if (file == null) throw new ArgumentNullException(nameof(file));

			await using var stream = file.OpenReadStream();
			var storedName = await _storage.SaveAsync(stream, file.FileName, cancellationToken).ConfigureAwait(false);

			var video = new Videos
			{
				Id = Guid.NewGuid(),
				Title = string.IsNullOrWhiteSpace(title) ? file.FileName : title,
				Description = description,
				UploadDate = DateTime.UtcNow,
				UploaderId = uploaderId,
				ThumbnailUrl = null,
				VideoUrl = storedName,
				ViewCount = 0,
				LikeCount = 0,
				DislikeCount = 0
			};

			_db.Videos.Add(video);
			await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

			return video;
		}
	}
}
