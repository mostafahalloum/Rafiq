using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rafiq.Data_Access;
using Rafiq.DTOs.Request;
using Rafiq.Models;
using Rafiq.Repositories.IRepositories;
using Rafiq.Utitlies;
using System.Threading.Tasks;

namespace Rafiq.Area.Admin
{
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    [Area("Admin")]

    public class OrderController : ControllerBase
    {


        private readonly IOrderRepository _orderRepository;

        public OrderController ( IOrderRepository orderRepository )
        {
            _orderRepository = orderRepository;
        }

        [HttpGet("")]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> GetAll ()
        {

            var orders = await _orderRepository.GetAsync(
               includes: [o => o.OrderItems, u => u.User],
                tracked : false);

            var result = orders.Select(o => new
            {
                o.Id,
                User = new
                {
                    o.User.Id,
                    o.User.FirstName,
                    o.User.LastName
                },
                o.TotalPrice,
                o.Status,
                o.CreateAt,
                ItemsCount = o.OrderItems.Count
            });

            return Ok(new
            {
                Orders = result,
            });
        }


        [HttpGet("{id}")]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> GetOne ( int id )
        {
            var order = await _orderRepository.GetFullData()
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == id);

            if ( order is null )
                return NotFound();

            var result = new
            {
                order.Id,
                User = new
                {
                    order.User.Id,
                    order.User.FirstName,
                    order.User.LastName
                },
                order.TotalPrice,
                order.Status,
                order.CreateAt,

                Items = order.OrderItems.Select(i => new
                {
                    i.ProductId,
                    ProductName = i.Product != null ? i.Product.Name : "Unknown",
                    i.Quantity,
                    i.Price
                }).ToList()
            };

            return Ok(result);
        }


        [HttpPut("{id}")]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> EditStatus ( int id, [FromBody] UpdateStatusRequest updateStatusRequest )
        {
            if ( !Enum.IsDefined(typeof(OrderStatus), updateStatusRequest.Status) )
                return BadRequest("Status invalid");

            var order = await _orderRepository.GetOneAsync(o => o.Id == id, tracked: true);

            if ( order is null )
                return NotFound();


            if ( order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Cancelled )
                return BadRequest("Cannot change status");

            bool isValid = (order.Status, updateStatusRequest.Status) switch
            {
                (OrderStatus.Pending, OrderStatus.Paid) => true,
                (OrderStatus.Pending, OrderStatus.Cancelled) => true,

                (OrderStatus.Paid, OrderStatus.Shipped) => true,
                (OrderStatus.Paid, OrderStatus.Cancelled) => true,

                (OrderStatus.Shipped, OrderStatus.Delivered) => true,

                _ => false
            };

            if ( !isValid )
                return BadRequest("Invalid status transition");

            order.Status = updateStatusRequest.Status;

            await _orderRepository.CommitAsync();

            return Ok(new
            {
                success = true,
                data = new
                {
                    order.Id,
                    order.Status
                }
            });
        }
    }
}
