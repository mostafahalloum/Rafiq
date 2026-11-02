using System.ComponentModel.DataAnnotations;

namespace Rafiq.DTOs.Request
{
    public class changePasswordProfileRequest
    {
        [DataType(DataType.Password)]
        public string? CurrentPassword { get; set; }
        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }
    }
}
