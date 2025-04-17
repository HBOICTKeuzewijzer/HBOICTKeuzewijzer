using HBOICTKeuzewijzer.Api.DAL;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using HBOICTKeuzewijzer.Api.Models;
using System.Reflection;

namespace HBOICTKeuzewijzer.Api.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = context.Set<T>();
        }

        public async Task<T?> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(T entity)
        {
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity != null) _dbSet.Remove(entity);
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _dbSet.AnyAsync(e => EF.Property<Guid>(e, "Id") == id);
        }

        public IQueryable<T> Queryable()
        {
            return _dbSet.AsQueryable();
        }

        public async Task<IEnumerable<T>> GetAllIncludingAsync(params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.ToListAsync();
        }


        public async Task<PaginatedResult<T>> GetPaginatedAsync(GetAllRequestQuery<T> request,
        params Expression<Func<T, object>>[] includes)
        {
            request.SetSortExpression();

            IQueryable<T> query = _dbSet;

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            if (!string.IsNullOrEmpty(request.Filter))
            {
                var filterExpression = BuildFilterExpression(request.Filter);
                query = query.Where(filterExpression);
            }

            if (request.SortExpression != null)
            {
                var methodName = request.SortDirection == Direction.Desc ? "OrderByDescending" : "OrderBy";
                var resultExpression = Expression.Call(
                    typeof(Queryable),
                    methodName,
                    new Type[] { typeof(T), request.SortExpression.ReturnType },
                    query.Expression,
                    Expression.Quote(request.SortExpression));

                query = query.Provider.CreateQuery<T>(resultExpression);
            }

            var totalCount = await query.CountAsync();

            if (request.Page.HasValue && request.PageSize.HasValue)
            {
                query = query
                    .Skip((request.Page.Value - 1) * request.PageSize.Value)
                    .Take(request.PageSize.Value);
            }

            return new PaginatedResult<T>
            {
                Items = await query.ToListAsync(),
                TotalCount = totalCount,
                Page = request.Page ?? 1,
                PageSize = request.PageSize ?? totalCount
            };
        }

        private Expression<Func<T, bool>> BuildFilterExpression(string filter)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var properties = typeof(T).GetProperties()
                .Where(p => p.PropertyType == typeof(string))
                .Select(p => Expression.Property(parameter, p.Name));

            var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            var filterExpression = properties
                .Select(p => Expression.Call(p, containsMethod, Expression.Constant(filter)))
                .Aggregate<Expression>((acc, next) => Expression.OrElse(acc, next));

            return Expression.Lambda<Func<T, bool>>(filterExpression, parameter);
        }
    }
}