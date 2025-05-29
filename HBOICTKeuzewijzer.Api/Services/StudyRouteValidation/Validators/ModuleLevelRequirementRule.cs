using HBOICTKeuzewijzer.Api.Models;
using HBOICTKeuzewijzer.Api.Models.OerRequirements;
using System.Text;

namespace HBOICTKeuzewijzer.Api.Services.StudyRouteValidation.Validators
{
    public class ModuleLevelRequirementRule : StudyRouteValidationRuleBase
    {
        public override Task Validate(Semester currentSemester, List<Semester> previousSemesters, Dictionary<string, List<string>> errors)
        {
            if (!GetParsedPrerequisite(currentSemester, out var modulePrerequisite)) return Task.CompletedTask;

            if (modulePrerequisite?.ModuleLevelRequirementGroups == null || modulePrerequisite.ModuleLevelRequirementGroups.Count == 0)
                return Task.CompletedTask;

            var moduleName = currentSemester.Module!.Name;
            var semesterId = currentSemester.Id.ToString();

            var res = CheckAllGroups(previousSemesters, modulePrerequisite.ModuleLevelRequirementGroups, moduleName);

            if (!res.AnyGroupPassed)
            {
                if (res.FailureResults.Count > 0)
                {
                    var sb = new StringBuilder();

                    sb.AppendLine($"Module: {moduleName} verwacht dat {(res.FailureResults.Count > 1 ? "een van de volgende groepen" : "de volgende groep")} module niveaus aanwezig is in de voorgaande semesters:");

                    for (int i = 0; i < res.FailureResults.Count; i++)
                    {
                        var failureResult = res.FailureResults[i];

                        sb.AppendLine();
                        sb.AppendLine($"Groep {i + 1}:");

                        foreach (var requirement in failureResult.Requirements)
                        {
                            if (failureResult.Failures.Any(g => requirement.Equals(g.Requirement) && g.Missing))
                            {
                                // Requirement failed since it was missing
                                sb.AppendLine($"- Niveau {requirement.Level} niet gevonden.");
                            }
                            else if (requirement.EcRequirement is not null && failureResult.Failures.Any(g => requirement.Equals(g.Requirement)))
                            {
                                // Requirement failed on credits
                                sb.AppendLine($"- Niveau {requirement.Level} wel gevonden maar gevonden voldoet niet aan de behaalde ec eis van {requirement.EcRequirement.RequiredAmount}.");
                            }
                            else
                            {
                                // Requirement did not fail
                                sb.AppendLine($"- Niveau {requirement.Level} gevonden.");
                            }
                        }
                    }

                    AddError(sb.ToString(), semesterId, errors);
                }
            }

            return Task.CompletedTask;
        }

        private GroupResult<ModuleLevelRequirement> CheckGroup(List<ModuleLevelRequirement> levelRequirements, List<Semester> relevantSemesters, string moduleName)
        {
            var result = new GroupResult<ModuleLevelRequirement>
            {
                Requirements = levelRequirements,
                GroupPassed = true
            };

            var checkedModules = new List<Module>();

            foreach (var levelRequirement in levelRequirements)
            {
                var candidates = relevantSemesters
                    .Where(s => s.Module != null &&
                                s.Module.Level == levelRequirement.Level &&
                                !checkedModules.Contains(s.Module))
                    .Select(s => new { Semester = s, Module = s.Module! })
                    .ToList();

                if (candidates.Count == 0)
                {
                    result.GroupPassed = false;
                    result.Failures.Add((levelRequirement, true));
                    continue;
                }

                var matched = false;
                var checkedModule = candidates.Last().Module;

                if (levelRequirement.EcRequirement != null)
                {
                    foreach (var candidate in candidates)
                    {
                        var semester = candidate.Semester;
                        var module = candidate.Module;

                        if (semester.AcquiredECs >= levelRequirement.EcRequirement.RequiredAmount)
                        {
                            matched = true;
                            checkedModule = module;
                            break;
                        }
                    }

                    if (!matched)
                    {
                        result.GroupPassed = false;
                        result.Failures.Add((levelRequirement, false));
                        checkedModules.Add(checkedModule);
                        continue;
                    }
                }

                checkedModules.Add(checkedModule);
            }

            return result;
        }

        private AllGroupsResult<ModuleLevelRequirement> CheckAllGroups(List<Semester> previousSemesters, List<ModuleLevelRequirementGroup> levelGroups, string moduleName)
        {
            var result = new AllGroupsResult<ModuleLevelRequirement>();

            var semestersWithModules = previousSemesters.Where(s => s.Module is not null).ToList();

            foreach (var levelGroup in levelGroups)
            {
                if (levelGroup.ModuleLevelRequirements.Count == 0) continue;

                var res = CheckGroup(levelGroup.ModuleLevelRequirements, semestersWithModules, moduleName);

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