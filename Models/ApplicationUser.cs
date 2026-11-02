using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Rafiq.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;
        [Required]
        public string LastName { get; set; } = string.Empty ;
        public string Gender { get; set; } = string.Empty ;
        public string? Address { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
        public int AiRequestsCount { get; set; } = 0;
        public DateTime AiRequestsDate { get; set; } = DateTime.UtcNow.Date;

        public string? ImageUrl { get; set; }

    }
}
