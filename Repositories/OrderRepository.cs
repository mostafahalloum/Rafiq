using Microsoft.EntityFrameworkCore;
using Rafiq.Data_Access;
using Rafiq.Models;
using Rafiq.Repositories.IRepositories;

namespace Rafiq.Repositories
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        private readonly ApplicationDbContext _context;

        public OrderRepository ( ApplicationDbContext context ) : base(context)
        {
            _context = context;
        }

        public IQueryable<Order> GetFullData ()
        {
            return _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Product);
        }
    }
}