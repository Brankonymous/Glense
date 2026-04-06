using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Glense.VideoCatalogue.Data;
using Glense.VideoCatalogue.GrpcClients;
using Glense.VideoCatalogue.Services;
using Glense.TestUtilities;

namespace VideoCatalogue.IntegrationTests;

/// <summary>
/// Custom WebApplicationFactory for VideoCatalogue integration tests.
/// Replaces DB with InMemory, mocks gRPC client and MassTransit, uses InMemoryVideoStorage.
/// </summary>
public class CustomVideoCatalogueFactory : WebApplicationFactory<Program>
{
    public Mock<IPublishEndpoint> MockPublishEndpoint { get; } = new();
    public Mock<IAccountGrpcClient> MockAccountGrpcClient { get; } = new();

    private readonly string _dbName = $"VideoCatTest_{Guid.NewGuid()}";

    public CustomVideoCatalogueFactory()
    {
        // Default mock behavior: return test usernames
        MockAccountGrpcClient
            .Setup(c => c.GetUsernameAsync(It.IsAny<Guid>()))
            .ReturnsAsync("test_uploader");

        MockAccountGrpcClient
            .Setup(c => c.GetUsernamesAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync((IEnumerable<Guid> ids) =>
                ids.Distinct().ToDictionary(id => id, _ => "test_uploader"));
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("JwtSettings:SecretKey", JwtTokenHelper.TestSecretKey);
        builder.UseSetting("JwtSettings:Issuer", JwtTokenHelper.Issuer);
        builder.UseSetting("JwtSettings:Audience", JwtTokenHelper.Audience);
        builder.UseSetting("InternalApiKey", "test-internal-api-key");

        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            // Remove real DbContext
            var dbDescriptors = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<VideoCatalogueDbContext>)
                         || d.ServiceType == typeof(VideoCatalogueDbContext))
                .ToList();
            foreach (var d in dbDescriptors) services.Remove(d);

            // Remove IAccountGrpcClient and all gRPC-related registrations
            var grpcDescriptors = services
                .Where(d => d.ServiceType == typeof(IAccountGrpcClient)
                         || d.ServiceType.FullName?.Contains("AccountGrpc") == true
                         || d.ImplementationType?.FullName?.Contains("AccountGrpc") == true
                         || d.ServiceType.FullName?.Contains("InternalApiKeyClientInterceptor") == true)
                .ToList();
            foreach (var d in grpcDescriptors) services.Remove(d);

            // Remove IVideoStorage
            var storageDescriptors = services
                .Where(d => d.ServiceType == typeof(IVideoStorage))
                .ToList();
            foreach (var d in storageDescriptors) services.Remove(d);

            // Remove MassTransit and register test replacements
            services.RemoveMassTransit();

            services.AddDbContext<VideoCatalogueDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));

            services.AddSingleton<IAccountGrpcClient>(MockAccountGrpcClient.Object);
            services.AddSingleton<IVideoStorage, InMemoryVideoStorage>();
            services.AddSingleton<IPublishEndpoint>(MockPublishEndpoint.Object);

            // Override JWT to use test secret key
            services.OverrideJwtAuthentication();
        });
    }
}
