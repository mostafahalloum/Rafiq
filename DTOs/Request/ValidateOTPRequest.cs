using System.ComponentModel.DataAnnotations;

namespace Rafiq.DTOs.Request
{
    public class ValidateOTPRequest
    {
        [Required]
        public string OTP { get; set; } =string.Empty;  
        public string Email { get; set; } = string.Empty;
    }
}
