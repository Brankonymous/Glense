using System.ComponentModel.DataAnnotations;

namespace Glense.AccountService.DTOs
{
    public class LoginDto
    {
        [Required]
        public string UsernameOrEmail { get; set; } = string.Empty;

        [Required] 
        public string Password { get; set; } = string.Empty;
    }
}
