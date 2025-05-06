using HBOICTKeuzewijzer.Api.Models;

namespace HBOICTKeuzewijzer.Api.Repositories
{
    public interface IStudyRouteRepository : IRepository<StudyRoute>
    {
        Task<List<StudyRoute>> GetForUser(ApplicationUser user);
    }
}
