using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rafiq.Data_Access;
using Rafiq.DTOs.Response;

namespace Rafiq.Area.Customer
{
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Area("Customer")]
    public class HomeController : ControllerBase
    {

            private ApplicationDbContext _context;
        public HomeController ( ApplicationDbContext context )
            {
                _context = context;
            }

            [HttpGet]
            public async Task<IActionResult> GetHome ()
            {
            var result = new HomeResponse
            {
                ImportantQuestions = await _context.ImportantQuestions
                    .AsNoTracking()
                    .OrderByDescending(x => x.Id)
                    .Take(5)
                    .Select(x => new QuestionResponse
                    {
                        Id = x.Id,
                        Answer = x.Answer,
                        Question = x.Question
                    }).ToListAsync(),

                Advertisements = await _context.Advertisements
                    .AsNoTracking()
                    .Select(x => new DidYouKnowAndAdvertisementResponse
                    {
                        Id = x.Id,
                        ImageUrl = x.Img
                    }).ToListAsync(),

                DidYouKnows = await _context.DidYouKnows
                    .AsNoTracking()
                    .Select(x => new DidYouKnowAndAdvertisementResponse
                    {
                        Id = x.Id,
                        ImageUrl = x.Img
                    }).ToListAsync(),

                TopSellingProducts = await _context.OrderItems
                    .AsNoTracking()
                    .GroupBy(x => new
                    {
                        x.Product.Id,
                        x.Product.Name,
                        x.Product.Price,
                        x.Product.Img
                    })
                    .Select(g => new ProductResponse
                    {
                        Id = g.Key.Id,
                        Name = g.Key.Name,
                        Price = g.Key.Price,
                        ImageUrl = g.Key.Img,
                        TotalSold = g.Sum(x => x.Quantity)
                    })
                    .OrderByDescending(x => x.TotalSold)
                    .Take(10)
                    .ToListAsync(),

                NewArrivals = await _context.Products
                    .AsNoTracking()
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(8)
                    .Select(g => new ProductResponse
                    {
                        Id = g.Id,
                        Name = g.Name,
                        Price = g.Price,
                        ImageUrl = g.Img
                    })
                    .ToListAsync()
            };

                return Ok(result);
            }
        }
    }
