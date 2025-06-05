using HBOICTKeuzewijzer.Api.Models;

namespace HBOICTKeuzewijzer.Api.Services.StudyRouteValidation.Validators
{
    public class PropaedeuticRule : StudyRouteValidationRuleBase
    {
        public override Task Validate(Semester currentSemester, List<Semester> previousSemesters, Dictionary<string, List<string>> errors)
        {
            // Parsing the prerequisite blob from database, if the parse method returns false there is no point in checking the requirement because there is no need to.
            if (!GetParsedPrerequisite(currentSemester, out var modulePrerequisite)) return Task.CompletedTask;

            // If parsing returned null the req is empty and so no need to check.
            if (modulePrerequisite is null) return Task.CompletedTask;

            // This rule is about the propaedeutic requirement and if there is none no need to check.
            if (modulePrerequisite.Propaedeutic is null) return Task.CompletedTask;

            // These are the semesters which are propaedeutic and so relevant to check.
            var pSemesters = previousSemesters
                .Where(s => s.Module is not null)
                .Where(s => s.Module!.IsPropaedeutic)
                .ToList();

            var semesterId = currentSemester.Id.ToString();

            // A resolved and completed P needs two modules.
            if (pSemesters.Count() < 2)
            {
                AddError($"Module: {currentSemester.Module!.Name} verwacht een voltooide propedeuse, minimaal 2 modules uit de P fase, {pSemesters.Count()} gevonden.", semesterId, errors);
            }

            // The credits received from the P modules need to be 60.
            var ecSum = pSemesters.Sum(s => s.AcquiredECs);
            if (ecSum < 60)
            {
                AddError($"Module: {currentSemester.Module!.Name} verwacht een voltooide propedeuse, minimaal 60 ec's behaald in de P fase, huidige ec's {ecSum}.", semesterId, errors);
            }

            return Task.CompletedTask;
        }
    }
}