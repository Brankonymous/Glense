using Glense.AccountService.DTOs;

namespace Glense.AccountService.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto);

        Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
        
        string GenerateJwtToken(Guid userId, string username, string email, string accountType);
    }
}
