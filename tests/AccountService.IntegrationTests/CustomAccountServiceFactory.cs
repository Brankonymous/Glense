using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Glense.AccountService.Data;
using Glense.TestUtilities;

namespace AccountService.IntegrationTests;

/// <summary>
/// Custom WebApplicationFactory for AccountService integration tests.
/// Overrides DB to InMemory, mocks MassTransit, and fixes JWT auth for tests.
/// </summary>
public class CustomAccountServiceFactory : WebApplicationFactory<Program>
{
    public Mock<IPublishEndpoint> MockPublishEndpoint { get; } = new();

    private readonly string _dbName = $"AccountTest_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Provide config values so Program.cs doesn't throw at startup
        builder.UseSetting("JwtSettings:SecretKey", JwtTokenHelper.TestSecretKey);
        builder.UseSetting("JwtSettings:Issuer", JwtTokenHelper.Issuer);
        builder.UseSetting("JwtSettings:Audience", JwtTokenHelper.Audience);
        builder.UseSetting("InternalApiKey", "test-internal-api-key");

        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            // Remove real DbContext registration and replace with InMemory
            var dbDescriptors = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<AccountDbContext>)
                         || d.ServiceType == typeof(AccountDbContext))
                .ToList();
            foreach (var d in dbDescriptors) services.Remove(d);

            services.AddDbContext<AccountDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));

            // Remove MassTransit and register mock IPublishEndpoint
            services.RemoveMassTransit();
            services.AddSingleton(MockPublishEndpoint.Object);

            // Override JWT to use test secret key (bypasses env var precedence in Program.cs)
            services.OverrideJwtAuthentication();
        });
    }
}
