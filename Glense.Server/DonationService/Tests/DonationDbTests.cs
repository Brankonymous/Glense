using Microsoft.EntityFrameworkCore;
using DonationService.Data;
using Xunit;

namespace DonationService.Tests;

public class DonationDbTests
{
    private DonationDbContext CreateContext()
    {
        // Always use in-memory database for unit tests
        var options = new DbContextOptionsBuilder<DonationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new DonationDbContext(options);
    }

    [Fact]
    public async Task GetAllWallets_ReturnsEmptyList()
    {
        using var context = CreateContext();
        var wallets = await context.Wallets.ToListAsync();
        Assert.NotNull(wallets);
        Assert.Empty(wallets);
    }

    [Fact]
    public async Task GetAllDonations_ReturnsEmptyList()
    {
        using var context = CreateContext();
        var donations = await context.Donations.ToListAsync();
        Assert.NotNull(donations);
        Assert.Empty(donations);
    }
}

