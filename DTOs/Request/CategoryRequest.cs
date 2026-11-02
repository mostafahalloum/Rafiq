using System.ComponentModel.DataAnnotations;

namespace Rafiq.DTOs.Request
{
    public class CategoryRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public IFormFile? Img { get; set; }
    }
}
