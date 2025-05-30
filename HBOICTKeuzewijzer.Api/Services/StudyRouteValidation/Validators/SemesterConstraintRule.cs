using HBOICTKeuzewijzer.Api.Models;
using HBOICTKeuzewijzer.Api.Models.OerRequirements;

namespace HBOICTKeuzewijzer.Api.Services.StudyRouteValidation.Validators
{
    public class SemesterConstraintRule : StudyRouteValidationRuleBase
    {
        public override Task Validate(Semester currentSemester, List<Semester> previousSemesters, Dictionary<string, List<string>> errors)
        {
            if (!GetParsedPrerequisite(currentSemester, out var modulePrerequisite)) return Task.CompletedTask;

            if (modulePrerequisite is null) return Task.CompletedTask;

            var currentSemesterConstraint = currentSemester.Index % 2 == 0 ? SemesterConstraint.First : SemesterConstraint.Second;

            if (modulePrerequisite.SemesterConstraint is not null &&
                modulePrerequisite.SemesterConstraint != currentSemesterConstraint)
            {
                AddError($"Module: {currentSemester.Module!.Name} kan alleen plaatsvinden in semester {(int)currentSemesterConstraint + 1}.", currentSemester.Id.ToString(), errors);
            }

            return Task.CompletedTask;
        }
    }
}