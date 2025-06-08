using HBOICTKeuzewijzer.Api.DAL;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using HBOICTKeuzewijzer.Api.Models;
using System.Reflection;

namespace HBOICTKeuzewijzer.Api.Repositories
{
    public class Repository<T> : IRepository<T> where T : class, IEntity
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
            var trackedEntity = _dbSet.Local.FirstOrDefault(e => e.Id.Equals(entity.Id));
            if (trackedEntity != null)
            {
                _context.Entry(trackedEntity).CurrentValues.SetValues(entity);
            }
            else
            {
                _dbSet.Attach(entity);
                _context.Entry(entity).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _dbSet.AnyAsync(e => EF.Property<Guid>(e, "Id") == id);
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

        public async Task<PaginatedResult<T>> GetPaginatedAsync(GetAllRequestQuery request,
            params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;

            //includes, hierdoor zorg ik ervoor dat alleen de desbetreffende dingen worden meegenomen
            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            // Dynamisch filter op 'Name' en 'Description' met LIKE-queries (indien aanwezig op het type)
            if (!string.IsNullOrEmpty(request.Filter))
            {
                var parameter = Expression.Parameter(typeof(T), "e");

                Expression? combined = null;

                foreach (var propName in new[] { "Name", "Description" })
                {
                    var property = typeof(T).GetProperty(propName);
                    if (property == null || property.PropertyType != typeof(string)) continue;

                    var propertyAccess = Expression.Property(parameter, property);
                    var nullCoalesced = Expression.Coalesce(propertyAccess, Expression.Constant(string.Empty));
                    var likeMethod = typeof(DbFunctionsExtensions).GetMethod("Like", new[] {
                        typeof(DbFunctions), typeof(string), typeof(string)
                    });

                    var likeCall = Expression.Call(
                        likeMethod!,
                        Expression.Constant(EF.Functions, typeof(DbFunctions)),
                        nullCoalesced,
                        Expression.Constant($"%{request.Filter}%")
                    );

                    combined = combined == null ? likeCall : Expression.OrElse(combined, likeCall);
                }

                if (combined != null)
                {
                    var lambda = Expression.Lambda<Func<T, bool>>(combined, parameter);
                    query = query.Where(lambda);
                }
            }

            //sortering
            if (!string.IsNullOrEmpty(request.SortColumn))
            {
                var propertyInfo = typeof(T).GetProperty(request.SortColumn,
                    BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (propertyInfo != null)
                {
                    var parameter = Expression.Parameter(typeof(T), "x");
                    var property = Expression.Property(parameter, propertyInfo);
                    var lambda = Expression.Lambda(property, parameter);

                    var methodName = request.SortDirection?.ToLower() == "desc"
                        ? "OrderByDescending"
                        : "OrderBy";

                    var resultExpression = Expression.Call(
                        typeof(Queryable),
                        methodName,
                        new Type[] { typeof(T), propertyInfo.PropertyType },
                        query.Expression,
                        Expression.Quote(lambda));

                    query = query.Provider.CreateQuery<T>(resultExpression);
                }
            }

            // Count voordat de paginering gebeurt (wil natuurlijk bepalen hoeveel resultaten per pagina)
            var totalCount = await query.CountAsync();

            // Pagination
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

        public IQueryable<T> Query()
        {
            return _dbSet.AsQueryable();
        }
    }
}