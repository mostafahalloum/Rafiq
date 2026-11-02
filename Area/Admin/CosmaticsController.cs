using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rafiq.Data_Access;
using Rafiq.DTOs.Request;
using Rafiq.DTOs.Response;
using Rafiq.Models;
using Rafiq.Repositories;
using Rafiq.Repositories.IRepositories;
using Rafiq.Utitlies;

[Route("api/[area]/[controller]")]
[ApiController]
[Authorize]
[Area("Admin")]
public class CosmaticsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ICosmeticesRepository _cosmaticsRepository;
    public CosmaticsController ( ApplicationDbContext context, ICosmeticesRepository cosmaticsRepository )
    {
        _context = context;
        _cosmaticsRepository = cosmaticsRepository;
    }

    [HttpGet("")]
    [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
    public async Task<IActionResult> GetAll ( [FromQuery] CosmeticsFilterRequest filter, [FromQuery] int page = 1 )
    {

        if ( page < 1 )
            page = 1;

        var query = _cosmaticsRepository.GetFullData();


        if ( !string.IsNullOrWhiteSpace(filter.Brand) )
            query = query.Where(p => p.Name.Contains(filter.Brand));

        if ( !string.IsNullOrWhiteSpace(filter.Name) )
            query = query.Where(p => p.Name.Contains(filter.Name));

        if ( filter.CategoryId.HasValue )
            query = query.Where(p => p.CategoryId == filter.CategoryId.Value);

        if ( filter.MinPrice.HasValue )
            query = query.Where(p => p.Price >= filter.MinPrice.Value);

        if ( filter.MaxPrice.HasValue )
            query = query.Where(p => p.Price <= filter.MaxPrice.Value);

        query = filter.Sort == "asc"
          ? query.OrderBy(x => x.CreatedAt)
          : query.OrderByDescending(x => x.CreatedAt);
        


        #region Pagination

        const int pageSize = 8;

        PaginationResponse paginationResponse = new();

        var totalCount = await query.CountAsync();

        paginationResponse.TotalItems = totalCount;
        paginationResponse.TotalPages = Math.Ceiling(totalCount / ( double ) pageSize);
        paginationResponse.CurrentPage = page;

        var result = await query
            .Skip(( page - 1 ) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        #endregion


        var respose = result.Select(item => new CosmeticsResponse
        {
            Id = item.Id,
            Brand = item.Brand,
            Name = item.Name,
            Description = item.Description,
            Price = item.Price,
            CategoryName = item.Category?.Name ?? "",
            Img = item.Img,
        }).ToList();

        return Ok(new
        {
            Data = respose,
            Pagination = paginationResponse
        });
    }

    [HttpGet("{id}")]
    [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
    public async Task<IActionResult> GetOne ( int id )
    {
        var cosmetic = await _cosmaticsRepository.GetFullData().FirstOrDefaultAsync(e => e.Id == id);

        if ( cosmetic is null )
            return NotFound();

        var response = new CosmeticsResponse
        {
            Id = cosmetic.Id,
            Brand = cosmetic.Brand,
            Name = cosmetic.Name,
            Description = cosmetic.Description,
            Price = cosmetic.Price,
            CategoryName = cosmetic.Category?.Name ?? "",
            Img = cosmetic.Img
        };

        return Ok(new
        {
            Data = response,
        });
    }


    [HttpPost("")]
    [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
    public async Task<IActionResult> Create ( cosmeticRequest CreateCosmeticRequest )
    {

        var cosmetic = new Cosmetices
        {
            Brand = CreateCosmeticRequest.Brand,
            Name = CreateCosmeticRequest.Name,
            Price = CreateCosmeticRequest.Price,
            Description = CreateCosmeticRequest.Description,
            CategoryId = CreateCosmeticRequest.CategoryId,
        };
          

        if ( CreateCosmeticRequest.Img is not null ) {
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(CreateCosmeticRequest.Img.FileName);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\Images", fileName);

            using ( var stream = System.IO.File.Create(filePath) ) {
                await CreateCosmeticRequest.Img.CopyToAsync(stream);
            }

            cosmetic.Img = fileName;

        }

        await _cosmaticsRepository.AddAsync(cosmetic);
        await _cosmaticsRepository.CommitAsync();

        return CreatedAtAction(nameof(GetOne), new { id = cosmetic.Id }, new
        {
            success_notifaction = "Add Cosmetic Successfully"
        });


    }

    [HttpPut("{id}")]
    [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
    public async Task<IActionResult> Edit ( int id, cosmeticRequest editCosmeticRequest )
    {

        var cosmeticInDb = await _cosmaticsRepository.GetFullData().FirstOrDefaultAsync(e => e.Id == id);
        if ( cosmeticInDb is null )
            return NotFound();

        if ( editCosmeticRequest.Img is not null ) {
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(editCosmeticRequest.Img.FileName);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\Images", fileName);


            using ( var stream = System.IO.File.Create(filePath) ) {
                await editCosmeticRequest.Img.CopyToAsync(stream);
            }

            var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\Images", cosmeticInDb.Img);
            if ( System.IO.File.Exists(oldPath) )
                System.IO.File.Delete(oldPath);


            cosmeticInDb.Img = fileName;


        }

        cosmeticInDb.Price = editCosmeticRequest.Price;
        cosmeticInDb.CategoryId = editCosmeticRequest.CategoryId;
        cosmeticInDb.Description = editCosmeticRequest.Description;
        cosmeticInDb.Name = editCosmeticRequest.Name;
        cosmeticInDb.Brand = editCosmeticRequest.Brand;

       
        _cosmaticsRepository.Update(cosmeticInDb);
        await _cosmaticsRepository.CommitAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
    public async Task<IActionResult> Delete ( int id )
    {

        var cosmeticInDb = await _cosmaticsRepository.GetOneAsync(e => e.Id == id);
        if ( cosmeticInDb is null )
            return NotFound();

        var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\Images", cosmeticInDb.Img);
        if ( System.IO.File.Exists(oldPath) )
            System.IO.File.Delete(oldPath);


        _cosmaticsRepository.Delete(cosmeticInDb);
        await _cosmaticsRepository.CommitAsync();

        return NoContent();
    }

}