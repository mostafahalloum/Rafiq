using Microsoft.EntityFrameworkCore;
using Rafiq.Data_Access;
using Rafiq.DTOs.Response;
using Rafiq.Models;
using Rafiq.Repositories.IRepositories;

namespace Rafiq.Repositories
{
    public class CosmeticesRepository : Repository<Cosmetices>, ICosmeticesRepository
    {
        private readonly ApplicationDbContext _context;

        public CosmeticesRepository ( ApplicationDbContext context ) : base(context)
        {
            _context = context;
        }

        public IQueryable<Cosmetices> GetFullData ()
        {

            return _context.Cosmetices
            .Include(p => p.Category)
            .AsNoTracking();
        }
    }
}
