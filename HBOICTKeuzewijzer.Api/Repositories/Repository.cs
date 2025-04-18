﻿using HBOICTKeuzewijzer.Api.DAL;
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

        public async Task<PaginatedResult<T>> GetPaginatedAsync(ModuleRequestQuery request,
            params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;

            //includes, hierdoor zorg ik ervoor dat alleen de desbetreffende dingen worden meegenomen
            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            //filtering
            if (!string.IsNullOrEmpty(request.Filter))
            {
                query = query.Where(e =>
                    EF.Functions.Like(e.GetType().GetProperty("Name").GetValue(e).ToString() ?? "", $"%{request.Filter}%") ||
                    EF.Functions.Like(e.GetType().GetProperty("Description").GetValue(e).ToString() ?? "", $"%{request.Filter}%"));
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
    }
}