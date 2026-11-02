using Microsoft.EntityFrameworkCore;
using Rafiq.Data_Access;
using Rafiq.Models;
using Rafiq.Repositories;
using Rafiq.Repositories.IRepositories;

public class CartRepository : Repository<Cart>, ICartRepository
{
    private readonly ApplicationDbContext _context;

    public CartRepository ( ApplicationDbContext context ) : base(context)
    {
        _context = context;
    }

    public async Task<Cart?> GetCartWithItemsAsync ( string userId )
    {
        return await _context.Carts
            .Include(c => c.CartItems)
            .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }
}