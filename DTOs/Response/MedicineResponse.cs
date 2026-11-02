namespace Rafiq.DTOs.Response
{
    public class MedicineResponse
    {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string ScientificName { get; set; } = string.Empty;
            public decimal Price { get; set; }
            public string Description { get; set; } = string.Empty;
            public string Manufacturer { get; set; } = string.Empty;
            public string Warning { get; set; } = string.Empty;
            public string ImageUrl { get; set; } = string.Empty;
            public string Dosage { get; set; } = string.Empty;
            public string CategoryName { get; set; } = string.Empty;
            public List<string> ActiveIngredients { get; set; } = new List<string>();

    }
}
