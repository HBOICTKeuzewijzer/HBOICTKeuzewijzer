using HBOICTKeuzewijzer.Api.Models;

namespace HBOICTKeuzewijzer.Api.Repositories
{
    public interface IModuleRepository : IRepository<Module>
    {
        Task FillWithRequiredModules(StudyRoute studyRoute);
    }
}