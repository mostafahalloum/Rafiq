using Rafiq.Models;

namespace Rafiq.Repositories.IRepositories
{
    public interface IOrderRepository : IRepository<Order>
    {
        IQueryable<Order> GetFullData ();
    }
}