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
    public class SubscriptionsController : ControllerBase
    {
        private readonly VideoCatalogueDbContext _db;

        public SubscriptionsController(VideoCatalogueDbContext db)
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
        public async Task<IActionResult> Subscribe([FromBody] DTOs.SubscribeRequestDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var subscriberId = GetCurrentUserId();
            var exists = await _db.Subscriptions.AnyAsync(s => s.SubscriberId == subscriberId && s.SubscribedToId == dto.SubscribedToId);
            if (exists) return Conflict("Already subscribed");

            var s = new Subscriptions { SubscriberId = subscriberId, SubscribedToId = dto.SubscribedToId, SubscriptionDate = System.DateTime.UtcNow };
            _db.Subscriptions.Add(s);
            await _db.SaveChangesAsync();

            var resp = new DTOs.SubscribeResponseDTO { SubscriberId = s.SubscriberId, SubscribedToId = s.SubscribedToId, SubscriptionDate = s.SubscriptionDate };
            return Created(string.Empty, resp);
        }

        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> Unsubscribe([FromBody] DTOs.SubscribeRequestDTO dto)
        {
            var subscriberId = GetCurrentUserId();
            var s = await _db.Subscriptions.FirstOrDefaultAsync(x => x.SubscriberId == subscriberId && x.SubscribedToId == dto.SubscribedToId);
            if (s == null) return NotFound();
            _db.Subscriptions.Remove(s);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
