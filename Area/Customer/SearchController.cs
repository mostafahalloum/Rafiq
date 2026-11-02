using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rafiq.Data_Access;

namespace Rafiq.Area.Customer
{
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Area("Customer")]
    public class SearchController : ControllerBase
    {

        private readonly ApplicationDbContext _context;

        public SearchController ( ApplicationDbContext context )
        {
            _context = context;
        }


        [HttpGet("Search")]
        public async Task<IActionResult> Search ( string? search )
        {
            if ( string.IsNullOrWhiteSpace(search) )
                return Ok(new { Data = new List<object>() });

            search = search.Trim();

            var medicines = await _context.Medicines
                .Include(m => m.Category)
                .Include(m => m.MedicineActiveIngredients)
                    .ThenInclude(ma => ma.activeIngredients)
                .Where(m =>
                    EF.Functions.Like(m.Name, $"%{search}%") ||
                    EF.Functions.Like(m.ScientificName, $"%{search}%") ||
                    m.MedicineActiveIngredients.Any(ma =>
                        EF.Functions.Like(ma.activeIngredients.Name, $"%{search}%")
                    )
                )
                .Select(m => new
                {
                    m.Id,
                    m.Name,
                    m.ScientificName,
                    m.Manufacturer,
                    m.Price,
                    m.Description,
                    m.Dosage,
                    m.Warning,
                    ImageUrl = m.Img,
                    CategoryName = m.Category != null ? m.Category.Name : "",

                    ActiveIngredients = m.MedicineActiveIngredients
                        .Select(ma => new
                        {
                            ma.activeIngredients.Id,
                            ma.activeIngredients.Name
                        })
                        .ToList(),

                    GeneralInfo =
                        "Medicine: " + m.Name +
                        ". Scientific name: " + m.ScientificName +
                        ". Used dosage: " + m.Dosage
                })
                .ToListAsync();

            return Ok(new { Data = medicines });
        }

        [HttpGet("{id}/similar")]
        public async Task<IActionResult> Similar ( int id )
        {
            var medicine = await _context.Medicines
                .Include(m => m.MedicineActiveIngredients)
                .FirstOrDefaultAsync(m => m.Id == id);

            if ( medicine == null )
                return NotFound();
            var ingredientIds = medicine.MedicineActiveIngredients
                .Select(x => x.ActiveIngredientsId)
                .ToList();

            var ingredientCount = ingredientIds.Count;

            var similarMedicines = await _context.Medicines
                .Include(m => m.Category)
                .Include(m => m.MedicineActiveIngredients)
                    .ThenInclude(ma => ma.activeIngredients)
                .Where(m =>
                    m.Id != id &&
                    m.MedicineActiveIngredients.Count == ingredientCount &&

                    ingredientIds.All(ai =>
                        m.MedicineActiveIngredients.Any(ma => ma.ActiveIngredientsId == ai)
                    )
                )
                .Select(m => new
                {
                    m.Id,
                    m.Name,
                    m.ScientificName,
                    m.Manufacturer,
                    m.Price,
                    m.Description,
                    m.Dosage,
                    m.Warning,
                    ImageUrl = m.Img,
                    CategoryName = m.Category != null ? m.Category.Name : "",

                    ActiveIngredients = m.MedicineActiveIngredients
                        .Select(ma => new
                        {
                            ma.activeIngredients.Id,
                            ma.activeIngredients.Name
                        })
                        .ToList()
                })
                .ToListAsync();

            return Ok(new { Data = similarMedicines });
        }
    }
}
