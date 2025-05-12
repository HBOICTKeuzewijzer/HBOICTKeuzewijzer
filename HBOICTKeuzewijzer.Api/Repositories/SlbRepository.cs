using HBOICTKeuzewijzer.Api.DAL;
using HBOICTKeuzewijzer.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace HBOICTKeuzewijzer.Api.Repositories
{
    public class SlbRepository : Repository<Slb>, ISlbRepository
    {
        public SlbRepository(AppDbContext context) : base(context)
        {
        }
        public async Task<IEnumerable<Slb>> GetAllWithUsersAsync()
        {
            return await _dbSet
                .Include(s => s.SlbApplicationUser)
                .Include(s => s.StudentApplicationUser)
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

        public async Task<PaginatedResult<ApplicationUser>> GetStudentsBySlbAsync(Guid slbId, int? page, int? pageSize)
        {
            var query = _dbSet
                .Include(s => s.StudentApplicationUser)
                .Where(s => s.SlbApplicationUserId == slbId)
                .Select(s => s.StudentApplicationUser);

            var totalCount = await query.CountAsync();

            if (page.HasValue && pageSize.HasValue)
            {
                query = query
                    .Skip((page.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value);
            }

            var items = await query.ToListAsync();

            return new PaginatedResult<ApplicationUser>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page ?? 1,
                PageSize = pageSize ?? totalCount
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
                throw new InvalidOperationException("Deze student is al gekoppeld aan deze SLB'er.");

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
                throw new InvalidOperationException("Relatie niet gevonden.");

            await DeleteAsync(relatie.Id);
            await _context.SaveChangesAsync(); 
        }
    }
}