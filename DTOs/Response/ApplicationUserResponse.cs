using System.ComponentModel.DataAnnotations;

namespace Rafiq.DTOs.Response
{
    public class ApplicationUserResponse
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }

    }
}
