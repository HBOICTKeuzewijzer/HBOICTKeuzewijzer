using HBOICTKeuzewijzer.Api.DAL;
using HBOICTKeuzewijzer.Api.Models;
using HBOICTKeuzewijzer.Api.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System.Linq.Expressions;
using System.Reflection;

namespace HBOICTKeuzewijzer.Api.Repositories
{
    public class SlbRepository : Repository<Slb>, ISlbRepository
    {
        public SlbRepository(AppDbContext context) : base(context)
        {
        }
        public async Task<IEnumerable<SlbDto>> GetAllWithUsersAsync()
        {
            return await _dbSet
                .Include(s => s.SlbApplicationUser)
                .Include(s => s.StudentApplicationUser)
                .GroupBy(s => s.SlbApplicationUser)
                .Select(group => new SlbDto
                {
                    Id = group.Key.Id,
                    DisplayName = group.Key.DisplayName,
                    Email = group.Key.Email,
                    Students = group
                        .Select(s => new StudentDto
                        {
                            Id = s.StudentApplicationUser.Id,
                            DisplayName = s.StudentApplicationUser.DisplayName,
                            Email = s.StudentApplicationUser.Email,
                            Code = s.StudentApplicationUser.Code,
                            Cohort = s.StudentApplicationUser.Cohort,
                            SlbId = s.SlbApplicationUserId
                        }).ToList()
                })
                .ToListAsync();
        }

        public async Task<Slb?> GetByStudentIdAsync(Guid studentId)
        {
            return await _dbSet
                .Include(s => s.SlbApplicationUser)
                .Include(s => s.StudentApplicationUser)
                .FirstOrDefaultAsync(s => s.StudentApplicationUserId == studentId);
        }

        public async Task<Slb?> GetBySlbIdAsync(Guid slbId)
        {
            return await _dbSet
                .Include(s => s.SlbApplicationUser)
                .Include(s => s.StudentApplicationUser)
                .FirstOrDefaultAsync(s => s.SlbApplicationUserId == slbId);
        }

        public async Task<PaginatedResult<StudentDto>> GetStudentsBySlbAsync(Guid slbId, GetAllRequestQuery request)
        {
            var query = _dbSet
                .Include(s => s.StudentApplicationUser)
                .Where(s => s.SlbApplicationUserId == slbId)
                .Select(s => s.StudentApplicationUser);

            // Filtering
            if (!string.IsNullOrWhiteSpace(request.Filter))
            {
                query = query.Where(u =>
                    EF.Functions.Like(u.DisplayName, $"%{request.Filter}%") ||
                    EF.Functions.Like(u.Email, $"%{request.Filter}%"));
            }

            // Sorting
            if (!string.IsNullOrEmpty(request.SortColumn))
            {
                var propertyInfo = typeof(ApplicationUser).GetProperty(request.SortColumn,
                    BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (propertyInfo != null)
                {
                    var parameter = Expression.Parameter(typeof(ApplicationUser), "x");
                    var property = Expression.Property(parameter, propertyInfo);
                    var lambda = Expression.Lambda(property, parameter);

                    var methodName = request.SortDirection?.ToLower() == "desc"
                        ? "OrderByDescending"
                        : "OrderBy";

                    var resultExpression = Expression.Call(
                        typeof(Queryable),
                        methodName,
                        new Type[] { typeof(ApplicationUser), propertyInfo.PropertyType },
                        query.Expression,
                        Expression.Quote(lambda));

                    query = query.Provider.CreateQuery<ApplicationUser>(resultExpression);
                }
            }

            // Count for pagination (How many results per page)
            var totalCount = await query.CountAsync();

            // Pagination
            if (request.Page.HasValue && request.PageSize.HasValue)
            {
                query = query
                    .Skip((request.Page.Value - 1) * request.PageSize.Value)
                    .Take(request.PageSize.Value);
            }

            var items = await query
                .Select(u => new StudentDto
                {
                    Id = u.Id,
                    DisplayName = u.DisplayName,
                    Email = u.Email,
                    Code = u.Code,
                    Cohort = u.Cohort
                })
        .ToListAsync();

            return new PaginatedResult<StudentDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = request.Page ?? 1,
                PageSize = request.PageSize ?? totalCount
            };
        }

        public async Task<bool> RelationExistsAsync(Guid slbId, Guid studentId)
        {
            return await _dbSet.AnyAsync(s => s.SlbApplicationUserId == slbId && s.StudentApplicationUserId == studentId);
        }

        public async Task<Slb?> GetRelationAsync(Guid slbId, Guid studentId)
        {
            return await _dbSet.FirstOrDefaultAsync(s => s.SlbApplicationUserId == slbId && s.StudentApplicationUserId == studentId);
        }

        public async Task AddSlbRelationAsync(Guid slbId, Guid studentId)
        {
            if (await RelationExistsAsync(slbId, studentId))
                throw new InvalidOperationException("This student is already linked to this Slb'er.");

            var slbRelatie = new Slb
            {
                Id = Guid.NewGuid(),
                SlbApplicationUserId = slbId,
                StudentApplicationUserId = studentId
            };

            await AddAsync(slbRelatie);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveSlbRelationAsync(Guid slbId, Guid studentId)
        {
            var relatie = await GetRelationAsync(slbId, studentId);

            if (relatie == null)
                throw new InvalidOperationException("Relation not found.");

            await DeleteAsync(relatie.Id);
            await _context.SaveChangesAsync();
        }

        public async Task<List<StudentDto>> GetAllRelationsForSlbAsync(Guid slbId)
        {
            return await _dbSet
                .Include(s => s.StudentApplicationUser)
                .Where(s => s.SlbApplicationUserId == slbId)
                .Select(s => new StudentDto
                {
                    Id = s.StudentApplicationUser.Id,
                    DisplayName = s.StudentApplicationUser.DisplayName,
                    Email = s.StudentApplicationUser.Email,
                    Code = s.StudentApplicationUser.Code,
                    Cohort = s.StudentApplicationUser.Cohort
                })
            .ToListAsync();
        }
    }
}