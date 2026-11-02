using Rafiq.Models;

namespace Rafiq.Repositories.IRepositories
{
    public interface ICosmeticesRepository : IRepository<Cosmetices>
    {
        IQueryable<Cosmetices> GetFullData ();
    }
}
