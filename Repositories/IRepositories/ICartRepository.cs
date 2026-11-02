using Rafiq.Models;

namespace Rafiq.Repositories.IRepositories
{
    public interface ICartRepository : IRepository<Cart>
    {
        Task<Cart?> GetCartWithItemsAsync ( string userId );
    }
}