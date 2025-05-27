using HBOICTKeuzewijzer.Api.Models;

namespace HBOICTKeuzewijzer.Api.Services.StudyRouteValidation.Validators
{
    public interface IStudyRouteValidationRule
    {
        Task Validate(Semester currentSemester, List<Semester> previousSemesters, Dictionary<string, List<string>> errors);
    }
}