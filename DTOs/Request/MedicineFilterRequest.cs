namespace Rafiq.DTOs.Request
{
    public class MedicineFilterRequest
    {
        public string? Name { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? CategoryId { get; set; }
        public string? Dosage { get; set; }
        public int? ActiveIngredientsId { get; set; }
        public string Sort { get; set; } = "desc";
    }
}
