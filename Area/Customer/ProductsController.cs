
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rafiq.Data_Access;
using Rafiq.Models;

namespace Rafiq.Area.Customer
{
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Area("Customer")]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductsController ( ApplicationDbContext context )
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll ( string? search, int page = 1 )
        {
            int pageSize = 9;

            var medicinesQuery = _context.Set<Medicine>()
                .Include(p => p.Category)
                .AsQueryable();

            var cosmeticsQuery = _context.Set<Cosmetices>()
                .Include(p => p.Category)
                .AsQueryable();

            if ( !string.IsNullOrWhiteSpace(search) ) {
                medicinesQuery = medicinesQuery.Where(p => EF.Functions.Like(p.Name, $"{search}%"));
                cosmeticsQuery = cosmeticsQuery.Where(p => EF.Functions.Like(p.Name, $"{search}%"));
            }

            var medicines = await medicinesQuery.ToListAsync();
            var cosmetics = await cosmeticsQuery.ToListAsync();

            var allProducts = new List<object>();

            allProducts.AddRange(medicines.Select(m => new
            {
                Type = "Medicine",
                m.Id,
                m.Name,
                m.Price,
                m.Description,
                m.Img,
                m.CreatedAt,
                m.ScientificName,
                m.Manufacturer,
                m.Warning,
                m.Dosage,
                m.Category
            }));

            allProducts.AddRange(cosmetics.Select(c => new
            {
                Type = "Cosmetices",
                c.Id,
                c.Name,
                c.Price,
                c.Description,
                c.Img,
                c.CreatedAt,
                c.Brand,
                c.Category
            }));

            var paged = allProducts
                .OrderBy(x => ( ( dynamic ) x ).Name)
                .Skip(( page - 1 ) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(new
            {
                totalPages = ( int ) Math.Ceiling(allProducts.Count / ( double ) pageSize),
                currentPage = page,
                Data = paged
            });
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> Get ( int id )
        {
            var medicine = await _context.Set<Medicine>()
                .Include(m => m.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if ( medicine != null ) {
                return Ok(new
                {
                    Type = "Medicine",
                    medicine.Id,
                    medicine.Name,
                    medicine.Description,
                    medicine.Price,
                    medicine.Img,
                    medicine.CreatedAt,
                    medicine.ScientificName,
                    medicine.Manufacturer,
                    medicine.Warning,
                    medicine.Dosage,
                    medicine.Category
                });
            }

            var cosmetic = await _context.Set<Cosmetices>()
                .Include(c => c.Category)
                .FirstOrDefaultAsync(c => c.Id == id);

            if ( cosmetic != null ) {
                return Ok(new
                {
                    Type = "Cosmetices",
                    cosmetic.Id,
                    cosmetic.Name,
                    cosmetic.Description,
                    cosmetic.Price,
                    cosmetic.Img,
                    cosmetic.CreatedAt,
                    cosmetic.Brand,
                    cosmetic.Category
                });
            }

            return NotFound();
        }

        [HttpGet("Autocomplete")]
        public IActionResult Autocomplete ( string? search )
        {
            var query = _context.Products.AsQueryable();

            if ( !string.IsNullOrWhiteSpace(search) ) {
                query = query.Where(p => EF.Functions.Like(p.Name, $"{search}%"));
            }

            var products = query
                .OrderBy(p => p.Name)
                .Take(6)
                .Select(p => new { p.Id, p.Name, p.Price, p.Img })
                .ToList();

            return Ok(products);
        }
    }
}
