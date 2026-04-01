using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Glense.ChatService.Data;

namespace ChatService.IntegrationTests;

/// <summary>
/// Custom WebApplicationFactory for ChatService integration tests.
/// Overrides DB to InMemory. ChatService has no external dependencies to mock.
/// </summary>
public class CustomChatServiceFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"ChatTest_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("JwtSettings:SecretKey", JwtTokenHelper.TestSecretKey);
        builder.UseSetting("JwtSettings:Issuer", JwtTokenHelper.Issuer);
        builder.UseSetting("JwtSettings:Audience", JwtTokenHelper.Audience);

        // Clear connection string to trigger InMemory DB fallback
        builder.UseSetting("ConnectionStrings:DefaultConnection", "");

        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            // Remove real DbContext and replace with InMemory
            var dbDescriptors = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<ChatDbContext>)
                         || d.ServiceType == typeof(ChatDbContext))
                .ToList();
            foreach (var d in dbDescriptors) services.Remove(d);

            services.AddDbContext<ChatDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));
        });
    }
}
