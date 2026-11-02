using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rafiq.Data_Access;
using Rafiq.DTOs.Request;
using Rafiq.Models;
using Rafiq.Repositories.IRepositories;

namespace Rafiq.Area.Customer
{
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Area("Customer")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ICartRepository _cartRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController (
            ICartRepository cartRepository,
            IOrderRepository orderRepository,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context )
        {
            _cartRepository = cartRepository;
            _orderRepository = orderRepository;
            _userManager = userManager;
            _context = context;
        }


        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout ([FromBody] CheckoutRequest CheckoutRequest
)
        {
            var user = await _userManager.GetUserAsync(User);
            if ( user == null )
                return Unauthorized();

            var cart = await _cartRepository.GetCartWithItemsAsync(user.Id);

            if ( cart is null || !cart.CartItems.Any() )
                return BadRequest("Cart is empty");

            var existingOrder = await _orderRepository.GetOneAsync(
                o => o.UserId == user.Id && o.Status == OrderStatus.Pending
            );


            if ( existingOrder != null ) {
                return Ok(new
                {
                    message = "You already have a pending order",
                    orderId = existingOrder.Id,
                    isExistingOrder = true
                });
            }

            var order = new Order
            {
                UserId = user.Id,
                Status = OrderStatus.Pending,
                CreateAt = DateTime.UtcNow,
                Address = CheckoutRequest.Address,
                Notes = CheckoutRequest.Notes,
                OrderItems = new List<OrderItem>()
            };

            foreach ( var item in cart.CartItems ) {
                order.OrderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Product.Price
                });
            }

            order.TotalPrice = order.OrderItems.Sum(i => i.Price * i.Quantity);

            await _orderRepository.AddAsync(order);

            _context.CartItems.RemoveRange(cart.CartItems);

            await _orderRepository.CommitAsync();

            return Ok(new
            {
                message = "Order created successfully",
                orderId = order.Id,
                totalPrice = order.TotalPrice
            });
        }


        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders ()
        {
            var user = await _userManager.GetUserAsync(User);
            if ( user == null )
                return Unauthorized();

            var orders = await _orderRepository
             .GetFullData()
             .Where(o => o.UserId == user.Id)
             .ToListAsync();

            return Ok(
            orders.Select(order => new
            {
                order.Id,
                order.TotalPrice,
                order.Status,
                CreateAt = order.CreateAt.ToUniversalTime(),
                order.Address
            })
        );
        }

 
        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderDetails ( int orderId )
        {
            var user = await _userManager.GetUserAsync(User);
            if ( user == null )
                return Unauthorized();

            var order = await _orderRepository
            .GetFullData()
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == user.Id);

            if ( order == null )
                return NotFound();

            var result = new
            {
                order.Id,
                order.TotalPrice,
                order.Status,
                CreateAt = order.CreateAt.ToUniversalTime(),

                order.Address,
                order.Notes,

                Items = order.OrderItems.Select(i => new
                {
                    i.ProductId,
                    ProductName = i.Product?.Name,
                    Image = i.Product?.Img,
                    i.Quantity,
                    i.Price
                })
            };

            return Ok(result);
        }

        [HttpPost("cancel/{orderId}")]
        public async Task<IActionResult> CancelOrder ( int orderId )
        {
            var user = await _userManager.GetUserAsync(User);
            if ( user == null )
                return Unauthorized();

            var order = await _orderRepository.GetOneAsync(
                o => o.Id == orderId && o.UserId == user.Id,
                tracked: true
            );

            if ( order == null )
                return NotFound();

            if ( order.Status != OrderStatus.Pending )
                return BadRequest("Cannot cancel this order");

            order.Status = OrderStatus.Cancelled;

            await _orderRepository.CommitAsync();

            return Ok(new { message = "Order cancelled successfully" });
        }
    }
}


