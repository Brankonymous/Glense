using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using DonationService.Consumers;
using DonationService.Data;
using Glense.Shared.Messages;
using Xunit;

namespace DonationService.IntegrationTests;

public class UserRegisteredEventConsumerTests
{
    private (UserRegisteredEventConsumer Consumer, DonationDbContext Db) CreateConsumer()
    {
        var options = new DbContextOptionsBuilder<DonationDbContext>()
            .UseInMemoryDatabase($"ConsumerTest_{Guid.NewGuid()}")
            .Options;
        var db = new DonationDbContext(options);
        var logger = Mock.Of<ILogger<UserRegisteredEventConsumer>>();
        var consumer = new UserRegisteredEventConsumer(db, logger);
        return (consumer, db);
    }

    [Fact]
    public async Task Consume_UserRegisteredEvent_CreatesWallet()
    {
        var (consumer, db) = CreateConsumer();
        var userId = Guid.NewGuid();

        var message = new UserRegisteredEvent
        {
            UserId = userId,
            Username = "newuser",
            Email = "newuser@example.com"
        };

        var mockContext = new Mock<ConsumeContext<UserRegisteredEvent>>();
        mockContext.Setup(c => c.Message).Returns(message);

        await consumer.Consume(mockContext.Object);

        var wallet = await db.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
        Assert.NotNull(wallet);
        Assert.Equal(0m, wallet.Balance);
    }

    [Fact]
    public async Task Consume_UserRegisteredEvent_Idempotent_NoDuplicateWallet()
    {
        var (consumer, db) = CreateConsumer();
        var userId = Guid.NewGuid();

        var message = new UserRegisteredEvent
        {
            UserId = userId,
            Username = "dupuser",
            Email = "dupuser@example.com"
        };

        var mockContext = new Mock<ConsumeContext<UserRegisteredEvent>>();
        mockContext.Setup(c => c.Message).Returns(message);

        // Consume twice
        await consumer.Consume(mockContext.Object);
        await consumer.Consume(mockContext.Object);

        var walletCount = await db.Wallets.CountAsync(w => w.UserId == userId);
        Assert.Equal(1, walletCount);
    }
}
