using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rafiq.Data_Access;
using Rafiq.DTOs.Request;
using Rafiq.DTOs.Response;
using Rafiq.Models;
using Rafiq.Repositories.IRepositories;
using Rafiq.Utitlies;
using System.Threading;

namespace Rafiq.Area.Admin
{
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Area("Admin")]
    [Authorize]
    public class MedicineController : ControllerBase
    {

        private readonly ApplicationDbContext _context;

        private readonly IMedicineRepository _medicineRepository;

        public MedicineController (
            ApplicationDbContext context,
            IMedicineRepository medicineRepository )
        {
            _medicineRepository = medicineRepository;
            _context = context;
        }

        [HttpGet("")]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> GetAll ( [FromQuery] MedicineFilterRequest filter, [FromQuery] int page = 1 )
        {
            var medicine = _medicineRepository.GetFullData(); 

            if ( !string.IsNullOrWhiteSpace(filter.Name) )
                medicine = medicine.Where(p => p.Name.Contains(filter.Name));

            if ( filter.CategoryId.HasValue )
                medicine = medicine.Where(p => p.CategoryId == filter.CategoryId.Value);

            if ( filter.MinPrice.HasValue )
                medicine = medicine.Where(p => p.Price >= filter.MinPrice.Value);

            if ( filter.MaxPrice.HasValue )
                medicine = medicine.Where(p => p.Price <= filter.MaxPrice.Value);

            if ( filter.ActiveIngredientsId.HasValue )
                medicine = medicine.Where(p =>
                    p.MedicineActiveIngredients
                     .Any(ma => ma.ActiveIngredientsId == filter.ActiveIngredientsId.Value));

            medicine = filter.Sort == "asc"
            ? medicine.OrderBy(x => x.CreatedAt)
            : medicine.OrderByDescending(x => x.CreatedAt);
            #region Pagination

            const int pageSize = 8;

            PaginationResponse paginationResponse = new();


            var totalCount = await medicine.CountAsync();

            paginationResponse.TotalItems = totalCount;
            paginationResponse.TotalPages = Math.Ceiling(totalCount / ( double ) pageSize);
            paginationResponse.CurrentPage = page;

            var result = await medicine
                .Skip(( page - 1 ) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            #endregion

            var resultDto = result.Select(m => new MedicineResponse
            {
                ImageUrl= m.Img,
                Id = m.Id,
                Name = m.Name,
                ScientificName = m.ScientificName,
                Price = m.Price,
                Description = m.Description,
                Manufacturer = m.Manufacturer,
                Warning = m.Warning,
                Dosage = m.Dosage,
                CategoryName = m.Category?.Name ?? "",
                ActiveIngredients = m.MedicineActiveIngredients
                         .Select(ma => ma.activeIngredients.Name)
                         .ToList()
            }).ToList();

            return Ok(new
            {
                Data = resultDto,
                Pagination = paginationResponse
            });
        }

        [HttpGet("{id}")]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> GetOne (int id)
        {
            var medicine = await _medicineRepository
            .GetFullData()
            .FirstOrDefaultAsync(e => e.Id == id);

            if ( medicine is null )
                return NotFound();

            var resultDto = new MedicineResponse
            {
                Id = medicine.Id,
                Name = medicine.Name,
                ScientificName = medicine.ScientificName,
                Price = medicine.Price,
                Description = medicine.Description,
                Manufacturer = medicine.Manufacturer,
                Warning = medicine.Warning,
                Dosage = medicine.Dosage,
                CategoryName = medicine.Category?.Name ?? "",
                ActiveIngredients = medicine.MedicineActiveIngredients
                         .Select(ma => ma.activeIngredients.Name)
                         .ToList(),
                ImageUrl = medicine.Img
            };

            return Ok(resultDto);
        }

        [HttpPost("")]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Create (MedicineRequest createMedicineRequest)
        {

            var medicine = new Medicine
            {
                Name = createMedicineRequest.Name,
                ScientificName = createMedicineRequest.ScientificName,
                Price = createMedicineRequest.Price,
                Description = createMedicineRequest.Description,
                Manufacturer = createMedicineRequest.Manufacturer,
                Warning = createMedicineRequest.Warning,
                CategoryId = createMedicineRequest.CategoryId,
                Dosage = createMedicineRequest.Dosage,
                MedicineActiveIngredients = createMedicineRequest.ActiveIngredients.Select(id =>
                    new MedicineActiveIngredients
                    {
                        ActiveIngredientsId = id
                    }).ToList()
            };

            if ( createMedicineRequest.Img is not null ) {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(createMedicineRequest.Img.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\Images", fileName);


                using ( var stream = System.IO.File.Create(filePath) ) {
                    await createMedicineRequest.Img.CopyToAsync(stream);
                }

                medicine.Img = fileName;

            }

            await _medicineRepository.AddAsync(medicine);
            await _medicineRepository.CommitAsync();

            return CreatedAtAction(nameof(GetOne), new { id = medicine.Id }, new
            {
                success_notifaction = "Add Medicine Successfully"
            });


        }
      
        [HttpPut("{id}")]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Edit (int id,MedicineRequest editMedicineRequest)
        {

            var MedicineInDb = await _medicineRepository.GetFullData().FirstOrDefaultAsync(e => e.Id == id);
            if ( MedicineInDb is null )
                return NotFound();

            if ( editMedicineRequest.Img is not null ) {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(editMedicineRequest.Img.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\Images", fileName);


                using ( var stream = System.IO.File.Create(filePath) ) {
                    await editMedicineRequest.Img.CopyToAsync(stream);
                }

                var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\Images", MedicineInDb.Img);
                if ( System.IO.File.Exists(oldPath) )
                    System.IO.File.Delete(oldPath);


                MedicineInDb.Img = fileName;


            }

            MedicineInDb.Manufacturer = editMedicineRequest.Manufacturer;
            MedicineInDb.Dosage = editMedicineRequest.Dosage;
            MedicineInDb.Price = editMedicineRequest.Price;
            MedicineInDb.ScientificName = editMedicineRequest.ScientificName;
            MedicineInDb.CategoryId = editMedicineRequest.CategoryId;
            MedicineInDb.Description = editMedicineRequest.Description;
            MedicineInDb.Name = editMedicineRequest.Name;

            _context.MedicineActiveIngredients
                .RemoveRange(MedicineInDb.MedicineActiveIngredients);

            MedicineInDb.MedicineActiveIngredients = editMedicineRequest.ActiveIngredients.Select(id =>
                new MedicineActiveIngredients
                {
                    MedicineId = MedicineInDb.Id,
                    ActiveIngredientsId = id
                }).ToList();


            _medicineRepository.Update(MedicineInDb);
            await _medicineRepository.CommitAsync();

            return NoContent();
        }
       
        [HttpDelete("{id}")]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Delete (int id)
        {

            var MedicineInDb = await _medicineRepository.GetOneAsync(e => e.Id == id);
            if ( MedicineInDb is null )
                return NotFound();

            var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\Images", MedicineInDb.Img);
            if ( System.IO.File.Exists(oldPath) )
                System.IO.File.Delete(oldPath);


            _medicineRepository.Delete(MedicineInDb);
            await _medicineRepository.CommitAsync();

            return NoContent();
        }
    }
}

