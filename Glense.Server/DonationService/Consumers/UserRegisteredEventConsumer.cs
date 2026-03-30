using MassTransit;
using Microsoft.EntityFrameworkCore;
using DonationService.Data;
using DonationService.Entities;
using Glense.Shared.Messages;

namespace DonationService.Consumers
{
    public class UserRegisteredEventConsumer : IConsumer<UserRegisteredEvent>
    {
        private readonly DonationDbContext _context;
        private readonly ILogger<UserRegisteredEventConsumer> _logger;

        public UserRegisteredEventConsumer(
            DonationDbContext context,
            ILogger<UserRegisteredEventConsumer> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
        {
            var msg = context.Message;
            _logger.LogInformation(
                "Received UserRegisteredEvent: UserId={UserId}, Username={Username}",
                msg.UserId, msg.Username);

            try
            {
                // Check if wallet already exists (idempotency)
                var existingWallet = await _context.Wallets
                    .FirstOrDefaultAsync(w => w.UserId == msg.UserId);

                if (existingWallet != null)
                {
                    _logger.LogInformation(
                        "Wallet already exists for user {UserId}, skipping creation", msg.UserId);
                    return;
                }

                var wallet = new Wallet
                {
                    Id = Guid.NewGuid(),
                    UserId = msg.UserId,
                    Balance = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Wallets.Add(wallet);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Wallet created for user {UserId} via UserRegisteredEvent", msg.UserId);
            }
            catch (DbUpdateException ex)
            {
                // Handle race condition - wallet may have been created by another consumer instance
                _logger.LogWarning(ex,
                    "DbUpdateException creating wallet for user {UserId}, likely already exists", msg.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to create wallet for user {UserId}", msg.UserId);
                throw;
            }
        }
    }
}
