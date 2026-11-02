using Rafiq.Models;

namespace Rafiq.DTOs.Response
{
    public class CosmeticsResponse
    {
        public int Id { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Img { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
    }
}
