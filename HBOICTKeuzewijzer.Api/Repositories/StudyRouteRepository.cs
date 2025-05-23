using HBOICTKeuzewijzer.Api.DAL;
using HBOICTKeuzewijzer.Api.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace HBOICTKeuzewijzer.Api.Repositories;

public class StudyRouteRepository : Repository<StudyRoute>, IStudyRouteRepository
{
    private const int MINIMUMSEMESTERS = 8;

    public StudyRouteRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<StudyRoute>> GetForUser(ApplicationUser user)
    {
        return await Query()
            .Where(s => s.ApplicationUserId == user.Id)
            .ToListAsync();
    }

    public async Task<StudyRoute> GetByIdWithSemesters(Guid id)
    {
        return await Query()
            .Include(s => s.Semesters!)
            .ThenInclude(s => s.Module)
            .ThenInclude(m => m.Category)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<bool> DeleteForUser(Guid id, ApplicationUser user)
    {
        var studyRoute = await Query()
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
        var existingNames = await Query()
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
            DisplayName = uniqueName,
            Semesters = []
        };

        for (var i = 0; i < MINIMUMSEMESTERS; i++)
        {
            studyRoute.Semesters.Add(new Semester
            {
                Index = i
            });
        }

        await AddAsync(studyRoute);

        return studyRoute;
    }

    public async Task<StudyRoute?> GetForUserById(ApplicationUser user, Guid id)
    {
        return await Query()
            .Include(s => s.Semesters!)
            .ThenInclude(s => s.Module)
            .ThenInclude(m => m.Category)
            .FirstOrDefaultAsync(r => r.Id == id && r.ApplicationUserId == user.Id);
    }
}