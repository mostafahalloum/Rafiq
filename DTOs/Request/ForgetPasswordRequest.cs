using System.ComponentModel.DataAnnotations;

namespace Rafiq.DTOs.Request
{
    public class ForgetPasswordRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
