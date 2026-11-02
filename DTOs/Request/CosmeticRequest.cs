using Rafiq.Models;

namespace Rafiq.DTOs.Request
{
    public class cosmeticRequest
    {
        public string Brand { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Description { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public IFormFile? Img { get; set; }
    }
}
