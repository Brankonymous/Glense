using System.Text;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Glense.TestUtilities;

/// <summary>
/// Extension methods for configuring test services in WebApplicationFactory.
/// </summary>
public static class ServiceCollectionTestExtensions
{
    /// <summary>
    /// Removes all MassTransit service registrations including hosted services,
    /// preventing the test host from attempting RabbitMQ connections.
    /// </summary>
    public static IServiceCollection RemoveMassTransit(this IServiceCollection services)
    {
        var descriptors = services
            .Where(d => d.ServiceType == typeof(IPublishEndpoint)
                     || d.ServiceType.FullName?.Contains("MassTransit") == true
                     || d.ImplementationType?.FullName?.Contains("MassTransit") == true
                     || (d.ServiceType == typeof(IHostedService)
                         && d.ImplementationType?.FullName?.Contains("MassTransit") == true))
            .ToList();

        foreach (var d in descriptors)
            services.Remove(d);

        return services;
    }

    /// <summary>
    /// Overrides JWT bearer authentication to use the test secret key.
    /// This ensures tokens from JwtTokenHelper are accepted regardless of
    /// what Program.cs reads from environment variables or config files.
    /// </summary>
    public static IServiceCollection OverrideJwtAuthentication(this IServiceCollection services)
    {
        services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = JwtTokenHelper.Issuer,
                ValidAudience = JwtTokenHelper.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(JwtTokenHelper.TestSecretKey))
            };
        });

        return services;
    }
}
