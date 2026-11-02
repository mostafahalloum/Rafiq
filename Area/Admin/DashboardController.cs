using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rafiq.Data_Access;
using Rafiq.Models;
using Rafiq.Utitlies;

[Route("api/[area]/[controller]")]
[ApiController]
[Area("Admin")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DashboardController ( ApplicationDbContext context )
    {
        _context = context;
    }

    [HttpGet]
    [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
    public async Task<IActionResult> GetDashboardData ()
    {
        var totalMedicines = await _context.Products.CountAsync();
        var totalOrders = await _context.Orders.CountAsync();
        var totalUsers = await _context.Users.CountAsync();

        var latestOrders = await _context.Orders
            .OrderByDescending(o => o.CreateAt)
            .Take(6)
            .Select(o => new
            {
                o.Id,
                Medicine = o.OrderItems.Select(i => i.Product.Name).FirstOrDefault(),
                Price = o.TotalPrice,
                o.Status
              
            })
            .ToListAsync();

        var topSelling = await _context.OrderItems
            .GroupBy(x => new
            {
                x.ProductId,
                x.Product.Name,
                x.Product.Img
            })
            .Select(g => new
            {
                g.Key.Name,
                g.Key.Img,
                Count = g.Sum(x => x.Quantity)
            })
            .OrderByDescending(x => x.Count)
            .Take(4)
            .ToListAsync();

                var sales = await _context.Orders
             .Where(o => o.Status == OrderStatus.Paid)
             .GroupBy(o => o.CreateAt.Date)
             .Select(g => new
             {
                 Date = g.Key,
                 Total = g.Sum(x => x.TotalPrice)
             })
             .OrderBy(x => x.Date)
             .ToListAsync();

        return Ok(new
        {
            totalMedicines,
            totalOrders,
            totalUsers,
            latestOrders,
            topSelling,
            sales
        });
    }
}