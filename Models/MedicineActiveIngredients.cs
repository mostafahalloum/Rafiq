using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rafiq.Models
{
    public class MedicineActiveIngredients
    {
        public int ActiveIngredientsId { get; set; }
        public ActiveIngredient activeIngredients { get; set; } = null!;
        public int MedicineId { get; set; }
        public Medicine medicine { get; set; } = null!;

     
    }
}
