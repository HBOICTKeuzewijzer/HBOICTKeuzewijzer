using HBOICTKeuzewijzer.Api.Models;
using System.Linq.Expressions;

namespace HBOICTKeuzewijzer.Api.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(Guid id);
        Task<IEnumerable<T>> GetAllAsync();
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        IQueryable<T> Queryable();
        Task<IEnumerable<T>> GetAllIncludingAsync(params Expression<Func<T, object>>[] includes);
        Task<PaginatedResult<T>> GetPaginatedAsync(
            GetAllRequestQuery request,
            params Expression<Func<T, object>>[] includes);
        IQueryable<T> Query();

    }
}
