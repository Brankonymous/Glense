// Import validation attributes
using System.ComponentModel.DataAnnotations;

namespace Glense.AccountService.DTOs
{
    public class UpdateProfileDto
    {

        [StringLength(50, MinimumLength = 3)]
        public string? Username { get; set; }

        [EmailAddress]
        //null means don't change email
        public string? Email { get; set; }

        public string? ProfilePictureUrl { get; set; }
    }
}
