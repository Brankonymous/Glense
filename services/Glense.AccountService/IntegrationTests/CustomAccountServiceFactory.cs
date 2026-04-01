using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Glense.AccountService.Data;
using Glense.AccountService.Services;

namespace AccountService.IntegrationTests;

/// <summary>
/// Custom WebApplicationFactory for AccountService integration tests.
/// Overrides DB to InMemory, mocks MassTransit, and suppresses Kestrel dual-port config.
/// </summary>
public class CustomAccountServiceFactory : WebApplicationFactory<Program>
{
    public Mock<IPublishEndpoint> MockPublishEndpoint { get; } = new();

    private readonly string _dbName = $"AccountTest_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set required config values so Program.cs doesn't throw
        builder.UseSetting("JwtSettings:SecretKey", JwtTokenHelper.TestSecretKey);
        builder.UseSetting("JwtSettings:Issuer", JwtTokenHelper.Issuer);
        builder.UseSetting("JwtSettings:Audience", JwtTokenHelper.Audience);
        builder.UseSetting("InternalApiKey", "test-internal-api-key");

        // Override Kestrel to suppress the dual-port config from Program.cs
        builder.ConfigureKestrel(options =>
        {
            options.ListenAnyIP(0, listenOptions =>
            {
                listenOptions.Protocols = HttpProtocols.Http1;
            });
        });

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

            // Remove MassTransit hosted services and all MassTransit registrations FIRST
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
