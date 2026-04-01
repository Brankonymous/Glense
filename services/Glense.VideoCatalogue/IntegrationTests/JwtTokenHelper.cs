using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace VideoCatalogue.IntegrationTests;

/// <summary>
/// Generates valid JWT tokens for integration tests using the same claims scheme
/// that AuthService.GenerateJwtToken() uses in production.
/// </summary>
public static class JwtTokenHelper
{
    public const string TestSecretKey = "ThisIsATestSecretKeyForIntegrationTests_MustBe32CharsOrMore!";
    public const string Issuer = "GlenseAccountService";
    public const string Audience = "GlenseApp";

    public static string GenerateToken(
        Guid userId,
        string username = "testuser",
        string email = "test@example.com",
        string accountType = "user")
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestSecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, username),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim("account_type", accountType),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
