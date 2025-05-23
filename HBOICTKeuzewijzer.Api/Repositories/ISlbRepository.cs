using HBOICTKeuzewijzer.Api.Models;
using HBOICTKeuzewijzer.Api.Dtos;

namespace HBOICTKeuzewijzer.Api.Repositories
{
    public interface ISlbRepository : IRepository<Slb>
    {
        Task<IEnumerable<SlbDto>> GetAllWithUsersAsync();
        Task<Slb?> GetByStudentIdAsync(Guid studentId);
        Task<Slb?> GetBySlbIdAsync(Guid slbId);
        Task<PaginatedResult<StudentDto>> GetStudentsBySlbAsync(Guid slbId, GetAllRequestQuery request);
        Task<bool> RelationExistsAsync(Guid slbId, Guid studentId);
        Task<Slb?> GetRelationAsync(Guid slbId, Guid studentId);
        Task AddSlbRelationAsync(Guid slbId, Guid studentId);
        Task RemoveSlbRelationAsync(Guid slbId, Guid studentId);

        Task<List<StudentDto>> GetAllRelationsForSlbAsync(Guid slbId);
    }
}
