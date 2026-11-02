using System.ComponentModel.DataAnnotations;

namespace Rafiq.DTOs.Request
{
    public class ResendEmailConfirmationRequest
    {
        [Required , EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
