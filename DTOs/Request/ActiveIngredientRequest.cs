using System.ComponentModel.DataAnnotations;

namespace Rafiq.DTOs.Request
{
    public class ActiveIngredientRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;
    }
}
