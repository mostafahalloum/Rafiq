
using System.Linq.Expressions;

namespace Rafiq.Repositories.IRepositories
{
    public interface IRepository<T>where T : class
    {

        //CRUD

        ///////////////////////////////
        ///////    Create
        ///////////////////////////////
        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);


        ///////////////////////////////
        ///////    Update
        ///////////////////////////////
        void Update(T entity);


        ///////////////////////////////
        ///////    Delete
        ///////////////////////////////
        void Delete(T entity);

        //////////////////////////////////////////
        ///////    Read all data or half data
        ////////////////////////////////////////// 
        Task<IEnumerable<T>> GetAsync(
            Expression<Func<T, bool>>? expression = null,
            Expression<Func<T, object>>[]? includes = null,
            bool tracked = true,
            CancellationToken cancellationToken = default);

        ///////////////////////////////
        ///////    Read one item
        ///////////////////////////////
        Task<T?> GetOneAsync(
            Expression<Func<T, bool>>? expression = null,
            Expression<Func<T, object>>[]? includes = null,
            bool tracked = true,
            CancellationToken cancellationToken = default);


        //////////////////////////////////////
        ///////    save Data in Database
        //////////////////////////////////////
        Task CommitAsync(CancellationToken cancellationToken = default);
    }
}
