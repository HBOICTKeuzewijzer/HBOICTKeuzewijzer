using HBOICTKeuzewijzer.Api.DAL;
using HBOICTKeuzewijzer.Api.Models;
using Microsoft.AspNetCore.Http.HttpResults;
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

    public async Task<bool> DeleteForUser(Guid id, ApplicationUser user)
    {
        var studyRoute = await Queryable()
            .FirstOrDefaultAsync(s => s.Id == id && s.ApplicationUserId == user.Id);

        if (studyRoute is null)
        {
            return false;
        }

        await DeleteAsync(studyRoute);

        return true;
    }

    public async Task<StudyRoute> AddWithUniqueDisplayName(ApplicationUser user, string baseDisplayName)
    {
        var existingNames = await Queryable()
            .Where(r => r.ApplicationUserId == user.Id)
            .Select(r => r.DisplayName)
            .ToListAsync();

        var nameSet = new HashSet<string>(existingNames, StringComparer.OrdinalIgnoreCase);

        var uniqueName = baseDisplayName;
        var suffix = 1;

        while (nameSet.Contains(uniqueName))
        {
            uniqueName = $"{baseDisplayName}_{suffix++}";
        }

        var studyRoute = new StudyRoute
        {
            ApplicationUserId = user.Id,
            DisplayName = uniqueName
        };

        await AddAsync(studyRoute);

        return studyRoute;
    }

    public async Task<StudyRoute?> GetForUserById(ApplicationUser user, Guid id)
    {
        return await Queryable()
            .FirstOrDefaultAsync(r => r.Id == id && r.ApplicationUserId == user.Id);
    }
}