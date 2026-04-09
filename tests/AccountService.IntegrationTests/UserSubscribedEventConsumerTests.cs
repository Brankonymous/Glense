using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Glense.AccountService.Consumers;
using Glense.AccountService.Data;
using Glense.AccountService.Models;
using Glense.AccountService.Services;
using Glense.Shared.Messages;
using Xunit;

namespace AccountService.IntegrationTests;

public class UserSubscribedEventConsumerTests
{
    private (UserSubscribedEventConsumer Consumer, AccountDbContext Db) CreateConsumer()
    {
        var options = new DbContextOptionsBuilder<AccountDbContext>()
            .UseInMemoryDatabase($"ConsumerTest_{Guid.NewGuid()}")
            .Options;
        var db = new AccountDbContext(options);
        var notificationService = new NotificationService(db);
        var logger = Mock.Of<ILogger<UserSubscribedEventConsumer>>();
        var consumer = new UserSubscribedEventConsumer(notificationService, logger);
        return (consumer, db);
    }

    [Fact]
    public async Task Consume_UserSubscribedEvent_CreatesNotification()
    {
        var (consumer, db) = CreateConsumer();

        // Seed the channel owner user
        var channelOwnerId = Guid.NewGuid();
        db.Users.Add(new User
        {
            Id = channelOwnerId,
            Username = "channel_owner",
            Email = "owner@example.com",
            PasswordHash = "hash",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var message = new UserSubscribedEvent
        {
            SubscriberId = Guid.NewGuid(),
            ChannelOwnerId = channelOwnerId,
            SubscriberUsername = "new_subscriber"
        };

        var mockContext = new Mock<ConsumeContext<UserSubscribedEvent>>();
        mockContext.Setup(c => c.Message).Returns(message);

        await consumer.Consume(mockContext.Object);

        var notification = await db.Notifications
            .FirstOrDefaultAsync(n => n.UserId == channelOwnerId && n.Type == "subscription");
        Assert.NotNull(notification);
        Assert.Contains("new_subscriber", notification.Message);
    }
}
