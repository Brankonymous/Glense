using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using DonationService.Data;
using DonationService.Services;

namespace DonationService.IntegrationTests;

/// <summary>
/// Custom WebApplicationFactory for DonationService integration tests.
/// Overrides DB to InMemory, mocks IAccountServiceClient and IPublishEndpoint.
/// </summary>
public class CustomDonationServiceFactory : WebApplicationFactory<Program>
{
    public Mock<IPublishEndpoint> MockPublishEndpoint { get; } = new();
    public Mock<IAccountServiceClient> MockAccountServiceClient { get; } = new();

    private readonly string _dbName = $"DonationTest_{Guid.NewGuid()}";

    public CustomDonationServiceFactory()
    {
        // Default mock behavior: return test usernames
        MockAccountServiceClient
            .Setup(c => c.GetUsernameAsync(It.IsAny<Guid>()))
            .ReturnsAsync("test_user");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("JwtSettings:SecretKey", JwtTokenHelper.TestSecretKey);
        builder.UseSetting("JwtSettings:Issuer", JwtTokenHelper.Issuer);
        builder.UseSetting("JwtSettings:Audience", JwtTokenHelper.Audience);

        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            // Remove real DbContext and replace with InMemory
            var dbDescriptors = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<DonationDbContext>)
                         || d.ServiceType == typeof(DonationDbContext))
                .ToList();
            foreach (var d in dbDescriptors) services.Remove(d);

            services.AddDbContext<DonationDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));

            // Replace IAccountServiceClient with mock (prevents real HTTP calls)
            var accountDescriptors = services
                .Where(d => d.ServiceType == typeof(IAccountServiceClient))
                .ToList();
            foreach (var d in accountDescriptors) services.Remove(d);
            services.AddSingleton(MockAccountServiceClient.Object);

            // Remove MassTransit hosted services and all registrations FIRST
            var massTransitDescriptors = services
                .Where(d => d.ServiceType == typeof(IPublishEndpoint)
                         || d.ServiceType.FullName?.Contains("MassTransit") == true
                         || d.ImplementationType?.FullName?.Contains("MassTransit") == true)
                .ToList();
            foreach (var d in massTransitDescriptors) services.Remove(d);

            // THEN register mock IPublishEndpoint
            services.AddSingleton(MockPublishEndpoint.Object);
        });
    }
}
