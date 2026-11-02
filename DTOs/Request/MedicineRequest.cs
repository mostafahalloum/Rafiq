using Rafiq.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Rafiq.DTOs.Request
{
    public class MedicineRequest
    {
        public string Name { get; set; } = string.Empty;
        public string ScientificName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public string Warning { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string Dosage { get; set; } = string.Empty;
        public List<int> ActiveIngredients { get; set; } = new();

   
        public IFormFile? Img { get; set; }
    }
}