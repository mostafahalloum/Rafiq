using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rafiq.DTOs.Response;
using Rafiq.Models;
using Rafiq.Repositories.IRepositories;

namespace Rafiq.Area.Customer
{
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Area("Customer")]
    public class StoreController : ControllerBase
    {
        private readonly IRepository<Category> _CategoryRepository;
        private readonly IRepository<Product> _ProductRepository;

        public StoreController (
            IRepository<Category> categoryRepository,
            IRepository<Product> productRepository )
        {
            _CategoryRepository = categoryRepository;
            _ProductRepository = productRepository;
        }

        [HttpGet("AllCategories")]
        public async Task<IActionResult> GetAllCategories ()
        {
            var categories = await _CategoryRepository.GetAsync(tracked: false);

            var response = categories.Select(c => new CategoryResponse
            {
                Id = c.Id,
                Name = c.Name,
                Img = c.Img,
            }).ToList();

            return Ok(new
            {
                data = response
            });
        }

        [HttpGet("Category/{categoryId}/Products")]
        public async Task<IActionResult> GetProductsByCategory ( int categoryId )
        {
            var products = await _ProductRepository.GetAsync(
                expression: p => p.CategoryId == categoryId,
                tracked: false
            );

            var response = products.Select(p => new ProductResponse
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                ImageUrl = p.Img
                
            }).ToList();

            return Ok(new
            {
                data = response
            });
        }
    }
}
