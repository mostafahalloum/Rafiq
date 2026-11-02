using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Rafiq.Data_Access;
using Rafiq.DTOs.Request;
using Rafiq.Models;
using Rafiq.Repositories.IRepositories;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Rafiq.Area.Customer
{
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    [Area("Customer")]
    public class CartController : ControllerBase
    {
        private readonly ICartRepository _cartRepository;
        private readonly IRepository<Product> _ProductRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController ( ICartRepository cartRepository, UserManager<ApplicationUser> userManager, IRepository<Product> ProductRepository )
        {
            _cartRepository = cartRepository;
            _userManager = userManager;
            _ProductRepository = ProductRepository;
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart ( CartRequest cartRequest )
        {
            var user = await _userManager.GetUserAsync(User);

            if ( user is null )
                return Unauthorized();


            var product = await _ProductRepository.GetOneAsync(p => p.Id == cartRequest.ProductId);

            if ( product is null )
                return NotFound("Product not found");

            var cart = await _cartRepository.GetOneAsync(c => c.UserId == user.Id, includes: [cart => cart.CartItems], tracked: true);

            if ( cart is null ) {
                cart = new Cart
                {
                    UserId = user.Id,
                    CartItems = new List<CartItem>()
                };

                await _cartRepository.AddAsync(cart);
            }

            var existingItem = cart.CartItems
        .FirstOrDefault(c => c.ProductId == cartRequest.ProductId);

            if ( existingItem is not null ) {
                existingItem.Quantity += cartRequest.Quantity;
            }
            else {
                var newItem = new CartItem
                {
                    ProductId = cartRequest.ProductId,
                    Quantity = cartRequest.Quantity,
                };

                cart.CartItems.Add(newItem);


            }
            await _cartRepository.CommitAsync();

            return Ok(new
            {
                message = "Added to cart",
                totalItems = cart.CartItems.Sum(i => i.Quantity)
            });
        }


        [HttpGet]
        public async Task<IActionResult> GetCart ()
        {
            var user = await _userManager.GetUserAsync(User);
            if ( user == null )
                return Unauthorized();

            var cart = await _cartRepository.GetCartWithItemsAsync(user.Id);

            if ( cart is null || !cart.CartItems.Any() )
                return Ok(new { message = "Cart is empty", totalPrice = 0, items = new List<object>() });


            var totalPrice = cart.CartItems.Sum(ci => ci.Product!.Price * ci.Quantity);


            var items = cart.CartItems.Select(ci => new
            {
                ci.Id,
                ci.ProductId,
                ProductName = ci.Product.Name,
                ProductPrice = ci.Product.Price,
                ci.Quantity,
                ci.Product.Img,
                SubTotal = ci.Product.Price * ci.Quantity
            }).ToList();

            return Ok(new
            {
                totalPrice,
                items
            });
        }


        [HttpPut("{Id}")]
        public async Task<IActionResult> UpdateCartItemQuantity ( int Id, int Quantity )
        {
            var user = await _userManager.GetUserAsync(User);
            if ( user == null )
                return Unauthorized();

            var cart = await _cartRepository.GetOneAsync(
                c => c.UserId == user.Id,
                includes: [c => c.CartItems],
                tracked: true
            );

            if ( cart is null )
                return NotFound("Cart not found");

            var item = cart.CartItems.FirstOrDefault(ci => ci.Id == Id); 
            if ( item == null )
                return NotFound("Cart item not found");

            if ( Quantity <= 0 ) {
                cart.CartItems.Remove(item);
            }
            else {
                item.Quantity = Quantity;
            }

            await _cartRepository.CommitAsync();

            return Ok(new 
            { 
                message = "Cart updated successfully" 
            });
        }



        [HttpDelete("{cartItemId}")]
        public async Task<IActionResult> DeleteCartItem ( int cartItemId )
        {
            var user = await _userManager.GetUserAsync(User);
            if ( user == null )
                return Unauthorized();

            var cart = await _cartRepository.GetOneAsync(
                c => c.UserId == user.Id,
                includes: [ c => c.CartItems ],
                tracked: true
            );

            if ( cart == null )
                return NotFound("Cart not found");

            var item = cart.CartItems.FirstOrDefault(ci => ci.Id == cartItemId);
            if ( item == null )
                return NotFound("Cart item not found");

            cart.CartItems.Remove(item);

            await _cartRepository.CommitAsync();

            return Ok(new 
            { 
                message = "Cart item removed successfully"
            });
        }

    }
}