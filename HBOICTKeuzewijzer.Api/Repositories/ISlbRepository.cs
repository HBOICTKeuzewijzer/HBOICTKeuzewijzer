using HBOICTKeuzewijzer.Api.Models;

namespace HBOICTKeuzewijzer.Api.Repositories
{
    public interface ISlbRepository : IRepository<Slb>
    {
        Task<IEnumerable<Slb>> GetAllWithUsersAsync();
        Task<Slb?> GetByStudentIdAsync(Guid studentId);
        Task<Slb?> GetBySlbIdAsync(Guid slbId);
        Task<PaginatedResult<ApplicationUser>> GetStudentsBySlbAsync(Guid slbId, int? page, int? pageSize);
        Task<bool> RelationExistsAsync(Guid slbId, Guid studentId);
        Task<Slb?> GetRelationAsync(Guid slbId, Guid studentId);
        Task AddSlbRelationAsync(Guid slbId, Guid studentId);
        Task RemoveSlbRelationAsync(Guid slbId, Guid studentId);
    }
}
