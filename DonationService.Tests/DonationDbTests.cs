using Microsoft.EntityFrameworkCore;
using DonationService.Data;
using DonationService.Entities;
using Xunit;

namespace DonationService.Tests;

public class DonationDbTests
{
    private static string? GetConnectionString()
    {
        return Environment.GetEnvironmentVariable("DONATION_DB_CONNECTION_STRING");
    }

    private DonationDbContext CreateContext()
    {
        var connectionString = GetConnectionString();

        var options = string.IsNullOrEmpty(connectionString)
            ? new DbContextOptionsBuilder<DonationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options
            : new DbContextOptionsBuilder<DonationDbContext>()
                .UseNpgsql(connectionString)
                .Options;

        return new DonationDbContext(options);
    }

    [Fact]
    public async Task GetAllWallets()
    {
        using var context = CreateContext();
        var wallets = await context.Wallets.ToListAsync();
        Assert.NotNull(wallets);
    }

    [Fact]
    public async Task GetAllDonations()
    {
        using var context = CreateContext();
        var donations = await context.Donations.ToListAsync();
        Assert.NotNull(donations);
    }
}
