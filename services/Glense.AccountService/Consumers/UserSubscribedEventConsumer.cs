using MassTransit;
using Glense.Shared.Messages;
using Glense.AccountService.Services;

namespace Glense.AccountService.Consumers
{
    public class UserSubscribedEventConsumer : IConsumer<UserSubscribedEvent>
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<UserSubscribedEventConsumer> _logger;

        public UserSubscribedEventConsumer(
            INotificationService notificationService,
            ILogger<UserSubscribedEventConsumer> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<UserSubscribedEvent> context)
        {
            var msg = context.Message;
            _logger.LogInformation(
                "Received UserSubscribedEvent: SubscriberId={SubscriberId}, ChannelOwnerId={ChannelOwnerId}",
                msg.SubscriberId, msg.ChannelOwnerId);

            try
            {
                await _notificationService.CreateNotificationAsync(
                    msg.ChannelOwnerId,
                    "New Subscriber!",
                    $"{msg.SubscriberUsername} subscribed to your channel!",
                    "subscription",
                    msg.SubscriberId);

                _logger.LogInformation(
                    "Subscription notification created for user {ChannelOwnerId}", msg.ChannelOwnerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to create subscription notification for user {ChannelOwnerId}", msg.ChannelOwnerId);
                throw;
            }
        }
    }
}
