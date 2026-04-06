using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Glense.AccountService.Consumers;
using Glense.AccountService.Data;
using Glense.AccountService.Models;
using Glense.AccountService.Services;
using Glense.Shared.Messages;
using Xunit;

namespace AccountService.IntegrationTests;

public class DonationMadeEventConsumerTests
{
    private (DonationMadeEventConsumer Consumer, AccountDbContext Db) CreateConsumer()
    {
        var options = new DbContextOptionsBuilder<AccountDbContext>()
            .UseInMemoryDatabase($"ConsumerTest_{Guid.NewGuid()}")
            .Options;
        var db = new AccountDbContext(options);
        var notificationService = new NotificationService(db);
        var logger = Mock.Of<ILogger<DonationMadeEventConsumer>>();
        var consumer = new DonationMadeEventConsumer(notificationService, logger);
        return (consumer, db);
    }

    [Fact]
    public async Task Consume_DonationMadeEvent_CreatesNotification()
    {
        var (consumer, db) = CreateConsumer();

        // Seed a user (the recipient)
        var recipientId = Guid.NewGuid();
        db.Users.Add(new User
        {
            Id = recipientId,
            Username = "recipient",
            Email = "recipient@example.com",
            PasswordHash = "hash",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var message = new DonationMadeEvent
        {
            DonorId = Guid.NewGuid(),
            RecipientId = recipientId,
            Amount = 25.00m,
            DonorUsername = "generous_donor"
        };

        var mockContext = new Mock<ConsumeContext<DonationMadeEvent>>();
        mockContext.Setup(c => c.Message).Returns(message);

        await consumer.Consume(mockContext.Object);

        var notification = await db.Notifications
            .FirstOrDefaultAsync(n => n.UserId == recipientId && n.Type == "donation");
        Assert.NotNull(notification);
        Assert.Contains("generous_donor", notification.Message);
        Assert.Contains("25.00", notification.Message);
    }
}
