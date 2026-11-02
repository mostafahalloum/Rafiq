using Rafiq.Models;

namespace Rafiq.Repositories.IRepositories
{
    public interface IMedicineRepository : IRepository<Medicine>
    {
        IQueryable<Medicine> GetFullData ();
    }
}
