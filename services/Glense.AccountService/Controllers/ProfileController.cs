using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Glense.AccountService.Data;
using Glense.AccountService.DTOs;

namespace Glense.AccountService.Controllers
{
    [ApiController]
    [Route("api/profile")]
    public class ProfileController : ControllerBase
    {
        private readonly AccountDbContext _context;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(AccountDbContext context, ILogger<ProfileController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim!);
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return NotFound();

                var dto = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    ProfilePictureUrl = user.ProfilePictureUrl,
                    AccountType = user.AccountType,
                    CreatedAt = user.CreatedAt
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        [HttpGet("{userId:guid}")]
        public async Task<IActionResult> GetById(Guid userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return NotFound();

                var dto = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    ProfilePictureUrl = user.ProfilePictureUrl,
                    AccountType = user.AccountType,
                    CreatedAt = user.CreatedAt
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by ID");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        [HttpPut("me")]
        public async Task<ActionResult<UserDto>> UpdateProfile([FromBody] UpdateProfileDto updateDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = await _context.Users.FindAsync(userId);

                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                // Update username if provided
                if (!string.IsNullOrWhiteSpace(updateDto.Username) && updateDto.Username != user.Username)
                {
                    var usernameExists = await _context.Users.AnyAsync(u => u.Username == updateDto.Username && u.Id != userId);
                    if (usernameExists)
                    {
                        return BadRequest(new { message = "Username already taken" });
                    }
                    user.Username = updateDto.Username;
                }

                // Update email if provided
                if (!string.IsNullOrWhiteSpace(updateDto.Email) && updateDto.Email != user.Email)
                {
                    var emailExists = await _context.Users.AnyAsync(u => u.Email == updateDto.Email && u.Id != userId);
                    if (emailExists)
                    {
                        return BadRequest(new { message = "Email already in use" });
                    }
                    user.Email = updateDto.Email;
                    user.IsVerified = false; // Reset verification when email changes
                }

                // Update profile picture if provided
                if (updateDto.ProfilePictureUrl != null)
                {
                    user.ProfilePictureUrl = updateDto.ProfilePictureUrl;
                }

                await _context.SaveChangesAsync();

                return Ok(new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    ProfilePictureUrl = user.ProfilePictureUrl,
                    AccountType = user.AccountType,
                    CreatedAt = user.CreatedAt,
                    IsVerified = user.IsVerified
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        [HttpDelete("me")]
        public async Task<IActionResult> DeleteAccount()
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = await _context.Users.FindAsync(userId);

                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                // Soft delete
                user.IsActive = false;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Account deactivated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting account");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }
    }
}
