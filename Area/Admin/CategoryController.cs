using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rafiq.DTOs.Request;
using Rafiq.DTOs.Response;
using Rafiq.Models;
using Rafiq.Repositories;
using Rafiq.Repositories.IRepositories;
using Rafiq.Utitlies;

namespace Rafiq.Area.Admin
{
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    [Area("Admin")]
    public class CategoryController : ControllerBase
    {
        private readonly IRepository<Category> _CategoryRepository;

        public CategoryController ( IRepository<Category> categoryRepository )
        {
            _CategoryRepository = categoryRepository;
        }

        [HttpGet]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> GetAll ()
        {
            var categories = await _CategoryRepository.GetAsync(tracked: false);

            var response = categories.Select(c => new CategoryResponse
            {
                Id = c.Id,
                Name = c.Name,
                Img = c.Img
            }).ToList();

            return Ok(new
            {
                data = response
            });
        }


        [HttpGet("{id}")]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> GetOne ( int id )
        {
            var category = await _CategoryRepository.GetOneAsync(e => e.Id == id);

            if ( category is null )
                return NotFound();


            var response = new CategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                Img = category.Img
            };
            return Ok(new
            {
                data = response
            });
        }



        [HttpPost]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Create ( [FromForm] CategoryRequest createCategoryRequest )
        {
            var category = new Category
            {
                Name = createCategoryRequest.Name
            };

            if ( createCategoryRequest.Img is not null ) {
                var fileName = Guid.NewGuid().ToString() +
                               Path.GetExtension(createCategoryRequest.Img.FileName);

                var filePath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot\\Images",
                    fileName);

                using ( var stream = System.IO.File.Create(filePath) ) {
                    await createCategoryRequest.Img.CopyToAsync(stream);
                }

                category.Img = fileName;
            }

            await _CategoryRepository.AddAsync(category);
            await _CategoryRepository.CommitAsync();

            return CreatedAtAction(nameof(GetOne), new { id = category.Id }, new
            {
                success_notification = "Category Added Successfully"
            });
        }


        [HttpPut("{id}")]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Edit (
       int id,
       [FromForm] CategoryRequest editCategoryRequest )
        {
            var categoryInDb = await _CategoryRepository.GetOneAsync(c => c.Id == id);

            if ( categoryInDb is null )
                return NotFound();

            categoryInDb.Name = editCategoryRequest.Name;

            if ( editCategoryRequest.Img is not null ) {
                var fileName = Guid.NewGuid().ToString() +
                               Path.GetExtension(editCategoryRequest.Img.FileName);

                var filePath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot\\Images",
                    fileName);

                using ( var stream = System.IO.File.Create(filePath) ) {
                    await editCategoryRequest.Img.CopyToAsync(stream);
                }

                if ( !string.IsNullOrEmpty(categoryInDb.Img) ) {
                    var oldPath = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot\\Images",
                        categoryInDb.Img);

                    if ( System.IO.File.Exists(oldPath) )
                        System.IO.File.Delete(oldPath);
                }

                categoryInDb.Img = fileName;
            }

            _CategoryRepository.Update(categoryInDb);
            await _CategoryRepository.CommitAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Delete ( int id )
        {
            var categoryInDb = await _CategoryRepository.GetOneAsync(c => c.Id == id);

            if ( categoryInDb is null )
                return NotFound();

            if ( !string.IsNullOrEmpty(categoryInDb.Img) ) {
                var oldPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot\\Images",
                    categoryInDb.Img);

                if ( System.IO.File.Exists(oldPath) )
                    System.IO.File.Delete(oldPath);
            }

            _CategoryRepository.Delete(categoryInDb);
            await _CategoryRepository.CommitAsync();

            return NoContent();
        }





    }
}
