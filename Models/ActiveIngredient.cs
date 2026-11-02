namespace Rafiq.Models
{
    public class ActiveIngredient
    {
        public int Id { get; set; }

        public string Name{ get; set; } = string.Empty;

        public List<MedicineActiveIngredients> MedicineActiveIngredients { get; set; }
            = new();
    }
}