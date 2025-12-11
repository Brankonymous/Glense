namespace Glense.AccountService.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }

        public string Username { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? ProfilePictureUrl { get; set; }

        public string AccountType { get; set; } = "user";

        public DateTime CreatedAt { get; set; }

        public bool IsVerified { get; set; }
    }
}
