using System.Linq.Expressions;
using HBOICTKeuzewijzer.Api.Models;

namespace HBOICTKeuzewijzer.Api.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(Guid id);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<IEnumerable<T>> GetAllIncludingAsync(params Expression<Func<T, object>>[] includes);
        Task<PaginatedResult<T>> GetPaginatedAsync(
        GetAllRequestQuery<T> request,
        params Expression<Func<T, object>>[] includes);
    }
}
