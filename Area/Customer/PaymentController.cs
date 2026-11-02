using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rafiq.Models;
using Rafiq.Repositories.IRepositories;
using Rafiq.Services;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Rafiq.Area.Customer
{
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Area("Customer")]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;
        private readonly INotificationService _telegramService;

        public PaymentController (
            IOrderRepository orderRepository,
            UserManager<ApplicationUser> userManager,
            IConfiguration config,
            INotificationService telegramService )
        {
            _orderRepository = orderRepository;
            _userManager = userManager;
            _config = config;
            _telegramService = telegramService;
        }

        [HttpPost("create-session/{orderId}")]
        public async Task<IActionResult> CreateSession ( int orderId, [FromQuery] string clientOrigin = "web" )
        {
            var user = await _userManager.GetUserAsync(User);

            if ( user == null )
                return Unauthorized();

            var order = await _orderRepository
                .GetFullData()
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == user.Id);

            if ( order == null )
                return NotFound("Order not found");

            if ( order.Status == OrderStatus.Paid )
                return BadRequest("Order already paid");

            if ( order.Status == OrderStatus.Cancelled )
                return BadRequest("Order cancelled");

            if ( order.Status != OrderStatus.Pending )
                return BadRequest("Invalid status");

            string successUrl = "https://rafiqpharma.com/success?orderId=" + order.Id;
            string cancelUrl = "https://rafiqpharma.com/cancel";

            if ( clientOrigin == "app" ) {
                successUrl = "rafiqapp://success?orderId=" + order.Id;
                cancelUrl = "rafiqapp://cancel";
            }

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                Mode = "payment",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                Metadata = new Dictionary<string, string>
                {
                    { "orderId", order.Id.ToString() }
                },
                LineItems = order.OrderItems.Select(item => new SessionLineItemOptions
                {
                    Quantity = item.Quantity,
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "egp",
                        UnitAmount = ( long ) ( item.Price * 100 ),
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product?.Name ?? "Product"
                        }
                    }
                }).ToList()
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            return Ok(new { url = session.Url });
        }

        [AllowAnonymous]
        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook ()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var endpointSecret = _config ["Stripe:WebhookSecret"];
            Event stripeEvent;

            try {
                stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers ["Stripe-Signature"],
                    endpointSecret
                );
            }
            catch ( Exception ex ) {
                return BadRequest(ex.Message);
            }

            if ( stripeEvent.Type == "checkout.session.completed" ) {
                var session = stripeEvent.Data.Object as Session;

                if ( session == null || !session.Metadata.ContainsKey("orderId") )
                    return Ok();

                var orderId = int.Parse(session.Metadata ["orderId"]);

                var order = await _orderRepository
                    .GetFullData()
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if ( order != null && order.Status == OrderStatus.Pending ) {
                    order.Status = OrderStatus.Paid;

             
                    order.PaidAt = DateTime.UtcNow;

                    await _orderRepository.CommitAsync();

                    string customerName = "Registered Customer";

                    if ( order.User != null )
                        customerName = $"{order.User.FirstName} {order.User.LastName}";
                    else {
                        var dbUser = await _userManager.FindByIdAsync(order.UserId);
                        if ( dbUser != null )
                            customerName = $"{dbUser.FirstName} {dbUser.LastName}";
                    }

                    await _telegramService.SendOrderAlertAsync(
                        order,
                        customerName,
                        "Paid & Confirmed 💳"
                    );
                }
            }

            return Ok();
        }
    }
}