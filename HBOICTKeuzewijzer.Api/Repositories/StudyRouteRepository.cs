using HBOICTKeuzewijzer.Api.DAL;
using HBOICTKeuzewijzer.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace HBOICTKeuzewijzer.Api.Repositories;

public class StudyRouteRepository : Repository<StudyRoute>, IStudyRouteRepository
{
    public StudyRouteRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<StudyRoute>> GetForUser(ApplicationUser user)
    {
        return await Queryable().Where(s => s.ApplicationUserId == user.Id).ToListAsync();
    }
}