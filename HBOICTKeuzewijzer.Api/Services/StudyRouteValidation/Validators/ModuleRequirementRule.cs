using HBOICTKeuzewijzer.Api.Models;
using HBOICTKeuzewijzer.Api.Models.OerRequirements;
using System.Text;

namespace HBOICTKeuzewijzer.Api.Services.StudyRouteValidation.Validators
{
    public class ModuleRequirementRule(Func<Guid, Task<Module?>> moduleResolver) : StudyRouteValidationRuleBase
    {
        public override async Task Validate(Semester currentSemester, List<Semester> previousSemesters, Dictionary<string, List<string>> errors)
        {
            if (!GetParsedPrerequisite(currentSemester, out var modulePrerequisite)) return;

            if (modulePrerequisite?.ModuleRequirementGroups == null || modulePrerequisite.ModuleRequirementGroups.Count == 0) return;

            var moduleName = currentSemester.Module!.Name;
            var semesterId = currentSemester.Id.ToString();

            var res = await CheckAllGroups(previousSemesters, modulePrerequisite.ModuleRequirementGroups, moduleName);

            if (!res.AnyGroupPassed)
            {
                if (res.FailureResults.Count > 0)
                {
                    var sb = new StringBuilder();

                    sb.AppendLine($"Module: {moduleName} verwacht dat {(res.FailureResults.Count > 1 ? "een van de volgende groepen" : "de volgende groep")} modules aanwezig is in de voorgaande semesters:");

                    for (int i = 0; i < res.FailureResults.Count; i++)
                    {
                        var failureResult = res.FailureResults[i];

                        sb.AppendLine();
                        sb.AppendLine($"Groep {i + 1}:");

                        foreach (var requirement in failureResult.Requirements)
                        {
                            var relevantModule = await moduleResolver(requirement.RelevantModuleId);

                            if (failureResult.Failures.Any(g => requirement.Equals(g.Requirement) && g.Missing))
                            {
                                sb.AppendLine($"- {relevantModule?.Name ?? "Module"} niet gevonden.");
                            }
                            else if (requirement.EcRequirement is not null && failureResult.Failures.Any(g => requirement.Equals(g.Requirement)))
                            {
                                sb.AppendLine($"- {relevantModule?.Name ?? "Module"} wel gevonden maar voldoet niet aan de behaalde ec eis van {requirement.EcRequirement.RequiredAmount}.");
                            }
                            else
                            {
                                sb.AppendLine($"- {relevantModule?.Name ?? "Module"} gevonden.");
                            }
                        }
                    }

                    AddError(sb.ToString(), semesterId, errors);
                }
            }
        }

        private async Task<GroupResult<ModuleRequirement>> CheckGroup(List<ModuleRequirement> moduleRequirements, List<Semester> relevantPreviousSemesters, string moduleName)
        {
            var result = new GroupResult<ModuleRequirement>
            {
                Requirements = moduleRequirements,
                GroupPassed = true
            };

            foreach (var moduleRequirement in moduleRequirements)
            {
                var relevantSemester = relevantPreviousSemesters
                    .FirstOrDefault(s => s.Module!.Id == moduleRequirement.RelevantModuleId);

                if (relevantSemester == null)
                {
                    result.GroupPassed = false;
                    result.Failures.Add((moduleRequirement, true));
                }
                else if (moduleRequirement.EcRequirement is not null &&
                    relevantSemester.AcquiredECs < moduleRequirement.EcRequirement.RequiredAmount)
                {
                    result.GroupPassed = false;
                    result.Failures.Add((moduleRequirement, false));
                }
            }

            return result;
        }

        private async Task<AllGroupsResult<ModuleRequirement>> CheckAllGroups(List<Semester> previousSemesters, List<ModuleRequirementGroup> moduleRequirementGroups, string moduleName)
        {
            var result = new AllGroupsResult<ModuleRequirement>();

            var semestersWithModules = previousSemesters.Where(s => s.Module is not null).ToList();

            foreach (var moduleRequirementGroup in moduleRequirementGroups)
            {
                if (moduleRequirementGroup.ModuleRequirements.Count == 0) continue;

                var res = await CheckGroup(moduleRequirementGroup.ModuleRequirements, semestersWithModules, moduleName);

                if (res.GroupPassed)
                {
                    result.AnyGroupPassed = true;
                    break;
                }

                result.FailureResults.Add(res);
            }

            return result;
        }
    }
}