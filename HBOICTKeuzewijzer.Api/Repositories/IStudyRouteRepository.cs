using HBOICTKeuzewijzer.Api.Models;

namespace HBOICTKeuzewijzer.Api.Repositories
{
    public interface IStudyRouteRepository : IRepository<StudyRoute>
    {
        Task<List<StudyRoute>> GetForUser(ApplicationUser user);
        Task<bool> DeleteForUser(Guid id, ApplicationUser user);
        Task<StudyRoute> AddWithUniqueDisplayName(ApplicationUser user, string baseDisplayName);
        Task<StudyRoute?> GetForUserById(ApplicationUser user, Guid id);
        Task<StudyRoute> GetByIdWithSemesters(Guid id);
    }
}
