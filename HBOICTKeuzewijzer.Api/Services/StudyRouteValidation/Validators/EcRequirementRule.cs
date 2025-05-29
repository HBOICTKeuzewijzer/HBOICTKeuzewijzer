using HBOICTKeuzewijzer.Api.Models;

namespace HBOICTKeuzewijzer.Api.Services.StudyRouteValidation.Validators
{
    public class EcRequirementRule : StudyRouteValidationRuleBase
    {
        public override Task Validate(Semester currentSemester, List<Semester> previousSemesters, Dictionary<string, List<string>> errors)
        {
            if (!GetParsedPrerequisite(currentSemester, out var modulePrerequisite)) return Task.CompletedTask;

            if (modulePrerequisite?.EcRequirements == null || modulePrerequisite.EcRequirements.Count == 0) return Task.CompletedTask;

            foreach (var ecRequirement in modulePrerequisite.EcRequirements)
            {
                bool Filter(Semester s) => s.Module != null && (!ecRequirement.Propaedeutic || s.Module.IsPropaedeutic);

                var relevantSemesters = previousSemesters.Where(s => s.Module != null && (!ecRequirement.Propaedeutic || s.Module.IsPropaedeutic)).ToList();
                var acquiredECs = relevantSemesters.Sum(s => s.AcquiredECs);
                var possibleECs = relevantSemesters.Sum(s => s.Module!.ECs);
                var moduleName = currentSemester.Module!.Name;
                var semesterId = currentSemester.Id.ToString();

                if (acquiredECs < ecRequirement.RequiredAmount)
                {
                    AddError($"Module: {moduleName} verwacht dat uit {(ecRequirement.Propaedeutic ? "propedeuse" : "voorgaande modules")} minimaal {ecRequirement.RequiredAmount} ec zijn behaald, huidige behaalde ec's is {acquiredECs}.",
                        semesterId, errors);
                }

                if (possibleECs < ecRequirement.RequiredAmount)
                {
                    AddError($"Module: {moduleName} verwacht dat uit {(ecRequirement.Propaedeutic ? "propedeuse" : "voorgaande modules")} minimaal {ecRequirement.RequiredAmount} ec behaalbaar zijn, huidige behaalbare ec's is {possibleECs}.",
                        semesterId, errors);
                }
            }

            return Task.CompletedTask;
        }
    }
}