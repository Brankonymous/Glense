using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Glense.AccountService.Data;
using Glense.AccountService.DTOs;
using Glense.AccountService.Models;

namespace Glense.AccountService.Services
{
    /// <summary>
    /// Implementation of the IAuthService interface
    /// Handles user registration, login, and JWT token generation
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly AccountDbContext _context;

        private readonly IConfiguration _configuration;

        public AuthService(AccountDbContext context, IConfiguration configuration) {
            _context = context;
            _configuration = configuration;
        }

        // Implement IAuthService.RegisterAsync(RegisterDto)
        public async Task<AuthResponseDto?> RegisterAsync(RegisterDto dto)
        {
            var exists = await _context.Users.AnyAsync(u => u.Username == dto.Username || u.Email == dto.Email);
            if (exists) return null;

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                AccountType = string.IsNullOrWhiteSpace(dto.AccountType) ? "user" : dto.AccountType,
                CreatedAt = DateTime.UtcNow,
                ProfilePictureUrl = dto.ProfilePictureUrl
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user.Id, user.Username, user.Email, user.AccountType);
            var userDto = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                ProfilePictureUrl = user.ProfilePictureUrl,
                AccountType = user.AccountType,
                CreatedAt = user.CreatedAt
            };

            return new AuthResponseDto
            {
                Token = token,
                User = userDto,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };
        }

        // Implement IAuthService.LoginAsync(LoginDto)
        public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
        {
            var user = await _context.Users
                .SingleOrDefaultAsync(u => u.Username == dto.UsernameOrEmail || u.Email == dto.UsernameOrEmail);

            if (user == null) return null;

            var valid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
            if (!valid) return null;

            var token = GenerateJwtToken(user.Id, user.Username, user.Email, user.AccountType);
            var userDto = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                ProfilePictureUrl = user.ProfilePictureUrl,
                AccountType = user.AccountType,
                CreatedAt = user.CreatedAt
            };

            return new AuthResponseDto
            {
                Token = token,
                User = userDto,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };
        }

        public string GenerateJwtToken(Guid userId, string username, string email, string accountType) {
            
            var jwtSettings = _configuration.GetSection("JwtSettings");

            var secretKey = jwtSettings["SecretKey"]
                ?? throw new InvalidOperationException("JWT SecretKey not configured");

            // Create a cryptographic key from the secret and create signing credentials
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                // "Sub" = Subject (who the token is about)
                // "UniqueName" = Username and Email
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, username),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim("account_type", accountType),

                // "Jti" = JWT ID (unique identifier for THIS token)
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
