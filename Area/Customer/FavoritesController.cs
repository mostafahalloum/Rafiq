using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rafiq.Data_Access;
using Rafiq.DTOs.Request;
using Rafiq.Models;
using System.Threading.Tasks;

namespace Rafiq.Area.Customer
{
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    [Area("Customer")]
    public class FavoritesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FavoritesController ( ApplicationDbContext context, UserManager<ApplicationUser> userManager )
        {
            _context = context;
            _userManager = userManager;
        }


        [HttpGet]
        public async Task<ActionResult> MyFavorites ()
        {
            var user = await _userManager.GetUserAsync(User);

            if ( user is null )
                return Unauthorized();

            var favorites = await _context.Favorites
                .Where(f => f.UserId == user.Id)
                .Select(f => new
                {
                    f.Id,
                    ProductId = f.Product.Id,
                    Name = f.Product.Name,
                    Price = f.Product.Price,
                    ImageUrl = f.Product.Img
                })
                .ToListAsync();

            return Ok(favorites);
        }

        [HttpPost("toggle")]
        public async Task<IActionResult> ToggleFavorite ( FavoriteRequest favoriteRequest )
        {
            var user = await _userManager.GetUserAsync(User);

            if ( user is null )
                return Unauthorized();

            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f =>
                    f.UserId == user.Id &&
                    f.ProductId == favoriteRequest.ProductId);

            if ( favorite != null ) {
                _context.Favorites.Remove(favorite);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    isFavorite = false,
                    message = "Removed from favorites"
                });
            }

            var newFavorite = new Favorites
            {
                UserId = user.Id,
                ProductId = favoriteRequest.ProductId,
                CreatedAt = DateTime.Now
            };

            _context.Favorites.Add(newFavorite);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                isFavorite = true,
                message = "Added to favorites"
            });
        }

    }
}
