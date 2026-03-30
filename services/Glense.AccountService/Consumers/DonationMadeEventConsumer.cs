using MassTransit;
using Glense.Shared.Messages;
using Glense.AccountService.Services;

namespace Glense.AccountService.Consumers
{
    public class DonationMadeEventConsumer : IConsumer<DonationMadeEvent>
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<DonationMadeEventConsumer> _logger;

        public DonationMadeEventConsumer(
            INotificationService notificationService,
            ILogger<DonationMadeEventConsumer> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<DonationMadeEvent> context)
        {
            var msg = context.Message;
            _logger.LogInformation(
                "Received DonationMadeEvent: DonorId={DonorId}, RecipientId={RecipientId}, Amount={Amount}",
                msg.DonorId, msg.RecipientId, msg.Amount);

            try
            {
                await _notificationService.CreateNotificationAsync(
                    msg.RecipientId,
                    "New Donation!",
                    $"{msg.DonorUsername} donated ${msg.Amount:F2} to you!",
                    "donation",
                    msg.DonorId);

                _logger.LogInformation(
                    "Donation notification created for user {RecipientId}", msg.RecipientId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to create donation notification for user {RecipientId}", msg.RecipientId);
                throw;
            }
        }
    }
}
