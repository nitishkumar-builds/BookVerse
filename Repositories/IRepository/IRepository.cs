using System.Linq.Expressions;

namespace BookVerse.DataAccess.Repository.IRepository
{
    public interface IRepository<T> where T : class
    {
        // Get a single entity by id
        T Get(Expression<Func<T, bool>> filter, string? includeProperties = null);

        // Get all entities, with optional filtering and includes
        IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null, string? includeProperties = null);

        void Add(T entity);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);
    }
}