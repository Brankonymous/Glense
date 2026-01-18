using System.Threading;
using System.Threading.Tasks;
using Glense.VideoCatalogue.Data;
using Glense.VideoCatalogue.Models;
using Microsoft.EntityFrameworkCore;

namespace Glense.VideoCatalogue.Services;
	public class Subscribe
	{
		private readonly VideoCatalogueDbContext _db;

		public Subscribe(VideoCatalogueDbContext db)
		{
			_db = db;
		}

		public async Task<Subscriptions> SubscribeAsync(int subscriberId, int subscribedToId, CancellationToken cancellationToken = default)
		{
			var exists = await _db.Subscriptions.FindAsync(new object[] { subscriberId, subscribedToId }, cancellationToken).ConfigureAwait(false);
			if (exists != null) return exists;

			var s = new Subscriptions { SubscriberId = subscriberId, SubscribedToId = subscribedToId, SubscriptionDate = System.DateTime.UtcNow };
			_db.Subscriptions.Add(s);
			await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
			return s;
		}

		public async Task<bool> UnsubscribeAsync(int subscriberId, int subscribedToId, CancellationToken cancellationToken = default)
		{
			var s = await _db.Subscriptions.FirstOrDefaultAsync(x => x.SubscriberId == subscriberId && x.SubscribedToId == subscribedToId, cancellationToken: cancellationToken).ConfigureAwait(false);
			if (s == null) return false;
			_db.Subscriptions.Remove(s);
			await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
			return true;
		}
}

