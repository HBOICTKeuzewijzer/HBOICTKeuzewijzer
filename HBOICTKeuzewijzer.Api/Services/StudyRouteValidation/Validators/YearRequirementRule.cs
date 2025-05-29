using HBOICTKeuzewijzer.Api.Models;

namespace HBOICTKeuzewijzer.Api.Services.StudyRouteValidation.Validators
{
    public class YearRequirementRule : StudyRouteValidationRuleBase
    {
        public override Task Validate(Semester currentSemester, List<Semester> previousSemesters,
            Dictionary<string, List<string>> errors)
        {
            if (!GetParsedPrerequisite(currentSemester, out var prerequisite)) return Task.CompletedTask;

            int year = (currentSemester.Index / 2) + 1;

            // check for the YearConstraints
            if (prerequisite.YearConstraints != null && prerequisite.YearConstraints.Any())
            {
                if (!prerequisite.YearConstraints.Contains(year))
                {
                    AddError($"Module: {currentSemester.Module!.Name} mag alleen gevolgd worden in jaar {string.Join(", ", prerequisite.YearConstraints)}, maar is geplaatst in jaar {year}.", currentSemester.Id.ToString(), errors);
                }
            }
            // check for the availablefromyear
            if (prerequisite.AvailableFromYear > 0 && year < prerequisite.AvailableFromYear)
            {
                AddError($"Module: {currentSemester.Module!.Name} is pas beschikbaar vanaf jaar {prerequisite.AvailableFromYear}, maar is nu gepland in jaar {year}.", currentSemester.Id.ToString(), errors);
            }

            return Task.CompletedTask;
        }
    }
}
