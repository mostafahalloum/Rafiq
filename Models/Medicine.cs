using System.Text.Json.Serialization;

namespace Rafiq.Models
{
    public class Medicine : Product
    {
        public string ScientificName { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public string Warning{ get; set; } = string.Empty;
        public string Dosage { get; set; } = string.Empty;
        public List<MedicineActiveIngredients> MedicineActiveIngredients { get; set; }
            = new();
    }
}