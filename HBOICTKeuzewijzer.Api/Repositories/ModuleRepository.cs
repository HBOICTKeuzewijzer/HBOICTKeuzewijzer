using HBOICTKeuzewijzer.Api.DAL;
using HBOICTKeuzewijzer.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace HBOICTKeuzewijzer.Api.Repositories
{
    public class ModuleRepository : Repository<Module>
    {
        public ModuleRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Module>> GetAllWithFilter(string filter)
        {
            return await Queryable().Where(m => m.Description.Contains(filter)).ToListAsync();
        }
    }
}
