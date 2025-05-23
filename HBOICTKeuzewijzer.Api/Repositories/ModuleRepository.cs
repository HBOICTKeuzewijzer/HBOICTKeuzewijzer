using HBOICTKeuzewijzer.Api.DAL;
using HBOICTKeuzewijzer.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace HBOICTKeuzewijzer.Api.Repositories
{
    public class ModuleRepository : Repository<Module>, IModuleRepository
    {
        public ModuleRepository(AppDbContext context) : base(context)
        {
        }

        public async Task FillWithRequiredModules(StudyRoute studyRoute)
        {
            var requiredModules = await Queryable().Where(m => m.Locked).ToListAsync();

            if (studyRoute.Semesters is null)
            {
                return;
            }

            var semesters = studyRoute.Semesters.ToList();

            foreach (var module in requiredModules)
            {
                var requiredSemester = module.LockedSemester ?? 0;

                semesters[requiredSemester].Module = module;
            }

            await _context.SaveChangesAsync();
        }
    }
}
