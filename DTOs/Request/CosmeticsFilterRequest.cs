namespace Rafiq.DTOs.Request
{
    public class CosmeticsFilterRequest
    {
        public string? Name { get; set; }
        public string? Brand { get; set; }

        public int? CategoryId { get; set; }

        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        public string Sort { get; set; } = "desc";
    }
}