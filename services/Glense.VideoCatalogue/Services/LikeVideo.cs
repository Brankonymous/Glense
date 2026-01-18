using System;
using System.Threading;
using System.Threading.Tasks;
using Glense.VideoCatalogue.Data;
using Glense.VideoCatalogue.Models;
using Microsoft.EntityFrameworkCore;

namespace Glense.VideoCatalogue.Services;
	public class LikeVideo
	{
		private readonly VideoCatalogueDbContext _db;

		public LikeVideo(VideoCatalogueDbContext db)
		{
			_db = db;
		}

		public async Task<(int LikeCount, int DislikeCount)> SetLikeAsync(int userId, Guid videoId, bool isLiked, CancellationToken cancellationToken = default)
		{
			var like = await _db.VideoLikes.FirstOrDefaultAsync(l => l.UserId == userId && l.VideoId == videoId, cancellationToken).ConfigureAwait(false);
			if (like == null)
			{
				like = new VideoLikes { UserId = userId, VideoId = videoId, IsLiked = isLiked };
				_db.VideoLikes.Add(like);
			}
			else
			{
				like.IsLiked = isLiked;
				_db.VideoLikes.Update(like);
			}

			var video = await _db.Videos.FirstOrDefaultAsync(v => v.Id == videoId, cancellationToken).ConfigureAwait(false);
			if (video != null)
			{
				var likes = await _db.VideoLikes.CountAsync(vl => vl.VideoId == videoId && vl.IsLiked, cancellationToken).ConfigureAwait(false);
				var dislikes = await _db.VideoLikes.CountAsync(vl => vl.VideoId == videoId && !vl.IsLiked, cancellationToken).ConfigureAwait(false);
				video.LikeCount = likes;
				video.DislikeCount = dislikes;
				_db.Videos.Update(video);
			}

			await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

			return (video?.LikeCount ?? 0, video?.DislikeCount ?? 0);
		}
}

