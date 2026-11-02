using Microsoft.EntityFrameworkCore;
using Rafiq.Data_Access;
using Rafiq.DTOs.Response;
using Rafiq.Models;
using Rafiq.Repositories.IRepositories;

namespace Rafiq.Repositories
{
    public class MedicineRepository : Repository<Medicine>, IMedicineRepository
    {
        private readonly ApplicationDbContext _context;

        public MedicineRepository ( ApplicationDbContext context ) : base(context)
        {
            _context = context;
        }

        public IQueryable<Medicine> GetFullData ()
        {

            return _context.Medicines
                .Include(e => e.Category)
                .Include(e => e.MedicineActiveIngredients)
                    .ThenInclude(ma => ma.activeIngredients)
                .AsNoTracking();
        }
    }
}
