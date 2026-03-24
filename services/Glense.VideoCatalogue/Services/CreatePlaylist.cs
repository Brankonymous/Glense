using System;
using System.Threading;
using System.Threading.Tasks;
using Glense.VideoCatalogue.Data;
using Glense.VideoCatalogue.Models;

namespace Glense.VideoCatalogue.Services;
	public class CreatePlaylist
	{
		private readonly VideoCatalogueDbContext _db;

		public CreatePlaylist(VideoCatalogueDbContext db)
		{
			_db = db;
		}

		public async Task<Playlists> CreateAsync(string name, string? description, Guid creatorId, CancellationToken cancellationToken = default)
		{
			var playlist = new Playlists
			{
				Id = Guid.NewGuid(),
				Name = name,
				Description = description,
				CreatorId = creatorId,
				CreationDate = DateTime.UtcNow
			};

			_db.Playlists.Add(playlist);
			await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
			return playlist;
		}
}

