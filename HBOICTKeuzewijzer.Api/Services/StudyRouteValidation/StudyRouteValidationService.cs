using HBOICTKeuzewijzer.Api.Models;
using HBOICTKeuzewijzer.Api.Models.OerRequirements;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using HBOICTKeuzewijzer.Api.Repositories;
using System.Text;
using static HBOICTKeuzewijzer.Api.Services.StudyRouteValidation.ModuleRequirementRule;
using System.Collections.Generic;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Linq;

namespace HBOICTKeuzewijzer.Api.Services.StudyRouteValidation;

public class StudyRouteValidationService : IStudyRouteValidationService
{
    private readonly List<IStudyRouteValidationRule> _rules;
    private readonly IRepository<Module> _moduleRepository;

    public StudyRouteValidationService(IRepository<Module> moduleRepository)
    {
        _moduleRepository = moduleRepository;

        _rules = new List<IStudyRouteValidationRule>
        {
            new PropaedeuticRule(),
            new PropaedeuticRule(),
            new EcRequirementRule(),
            new ModuleRequirementRule(ModuleResolver),
            new ModuleLevelRequirementRule()
        };
    }

    public StudyRouteValidationService(List<IStudyRouteValidationRule> rules)
    {
        _rules = rules;
    }

    private Task<Module?> ModuleResolver(Guid moduleId)
    {
        return _moduleRepository.GetByIdAsync(moduleId);
    }

    /// <summary>
    /// Validates a given <see cref="StudyRoute"/> instance against predefined rules set by Windesheim,
    /// including module prerequisites, EC requirements, semester constraints, and level requirements.
    /// </summary>
    /// <param name="routeToValidate">The study route to validate, including all semesters and modules.</param>
    /// <returns>A <see cref="ValidationProblemDetails"/> object containing validation errors if any rules are violated;
    /// otherwise, <c>null</c> if the route passes all validation checks or is structurally incomplete (e.g., no semesters).</returns>
    /// <remarks>
    /// This method iterates over all semesters in the provided route, extracting each module's prerequisites.
    /// If any rules (such as EC thresholds, required module completions, level requirements, or semester constraints)
    /// are violated, they are collected and returned as structured validation errors.
    /// 
    /// This method is intended to be used as part of backend API validation logic, providing standardized feedback
    /// in line with RFC 7807 Problem Details for HTTP APIs.
    /// </remarks>
    public async Task<ValidationProblemDetails?> ValidateRoute(StudyRoute routeToValidate)
    {
        ArgumentNullException.ThrowIfNull(routeToValidate);

        if (routeToValidate.Semesters is null || routeToValidate.Semesters.Count == 0)
            return null;

        var validationResult = new ValidationProblemDetails
        {
            Title = "One or more validation errors occurred.",
            Status = 400,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
        };

        var errors = new Dictionary<string, List<string>>();
        var previousSemesters = new List<Semester>();

        foreach (var semester in routeToValidate.Semesters)
        {
            foreach (var rule in _rules)
            {
                await rule.Validate(semester, previousSemesters, errors);
            }

            previousSemesters.Add(semester);
        }

        if (errors.Count > 0)
        {
            foreach (var kvp in errors)
            {
                validationResult.Errors[kvp.Key] = kvp.Value.ToArray();
            }

            return validationResult;
        }

        return null;
    }
}

public interface IStudyRouteValidationRule
{
    Task Validate(Semester currentSemester, List<Semester> previousSemesters, Dictionary<string, List<string>> errors);
}

public abstract class StudyRouteValidationRuleBase : IStudyRouteValidationRule
{
    public abstract Task Validate(Semester currentSemester, List<Semester> previousSemesters, Dictionary<string, List<string>> errors);

    protected bool GetParsedPrerequisite(Semester semester, out ModulePrerequisite? modulePrerequisite)
    {
        if (semester.Module is null || string.IsNullOrEmpty(semester.Module.PrerequisiteJson))
        {
            modulePrerequisite = null;
            return false;
        }

        modulePrerequisite = JsonConvert.DeserializeObject<ModulePrerequisite>(semester.Module.PrerequisiteJson);
        return true;
    }

    protected void AddError(string errorText, string key, Dictionary<string, List<string>> errors)
    {
        if (!errors.ContainsKey(key))
        {
            errors[key] = new List<string>();
        }

        errors[key].Add(errorText);
    }
}

public class PropaedeuticRule : StudyRouteValidationRuleBase
{
    public override Task Validate(Semester currentSemester, List<Semester> previousSemesters, Dictionary<string, List<string>> errors)
    {
        if (!GetParsedPrerequisite(currentSemester, out var modulePrerequisite)) return Task.CompletedTask;

        if (modulePrerequisite is null) return Task.CompletedTask;

        if (!modulePrerequisite.Propaedeutic) return Task.CompletedTask;

        var pSemesters = previousSemesters
            .Where(s => s.Module is not null)
            .Where(s => s.Module!.IsPropaedeutic)
            .ToList();
        var semesterId = currentSemester.Id.ToString();

        if (pSemesters.Count() < 2)
        {
            AddError($"Module: {currentSemester.Module!.Name} verwacht een voltooide propedeuse, minimaal 2 modules uit de P fase, {pSemesters.Count()} gevonden.", semesterId, errors);
        }

        var ecSum = pSemesters.Sum(s => s.AcquiredECs);
        if (ecSum < 60)
        {
            AddError($"Module: {currentSemester.Module!.Name} verwacht een voltooide propedeuse, minimaal 60 ec's behaald in de P fase, huidige ec's {ecSum}", semesterId, errors);
        }

        return Task.CompletedTask;
    }
}

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
                AddError($"Module: {moduleName} verwacht dat uit {(ecRequirement.Propaedeutic ? "propedeuse" : "voorgaande modules")} minimaal {ecRequirement.RequiredAmount} ec zijn behaald, huidige behaalde ec's is {acquiredECs}",
                    semesterId, errors);
            }

            if (possibleECs < ecRequirement.RequiredAmount)
            {
                AddError($"Module: {moduleName} verwacht dat uit {(ecRequirement.Propaedeutic ? "propedeuse" : "voorgaande modules")} minimaal {ecRequirement.RequiredAmount} ec behaalbaar zijn, huidige behaalbare ec's is {possibleECs}",
                    semesterId, errors);
            }
        }

        return Task.CompletedTask;
    }
}

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
                        if (failureResult.Failures.Any(g => requirement.Equals(g.LevelRequirement) && g.Missing))
                        {
                            // Requirement failed since it was missing
                            sb.AppendLine($"- Niveau {requirement.Level} niet gevonden.");
                        }
                        else if (requirement.EcRequirement is not null && failureResult.Failures.Any(g => requirement.Equals(g.LevelRequirement)))
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

    private GroupResult CheckGroup(List<ModuleLevelRequirement> levelRequirements, List<Semester> relevantSemesters, string moduleName)
    {
        var result = new GroupResult();
        result.Requirements = levelRequirements;
        var checkedModules = new List<Module>();

        foreach (var levelRequirement in levelRequirements)
        {
            // Only consider modules not already used
            var candidates = relevantSemesters
                .Where(s => s.Module != null && s.Module.Level == levelRequirement.Level && !checkedModules.Contains(s.Module))
                .Select(s => new { Semester = s, Module = s.Module! })
                .ToList();


            // No modules found for level 
            if (candidates.Count == 0)
            {
                result.GroupPassed = false;
                result.Failures.Add((levelRequirement, true));
                continue;
            }

            var checkedModule = candidates.Last().Module;
            if (levelRequirement.EcRequirement is not null)
            {
                bool matched = false;
                foreach (var candidate in candidates)
                {
                    var semester = candidate.Semester;
                    var module = candidate.Module;

                    // If EC requirement is present, check if it's satisfied
                    if (levelRequirement.EcRequirement is not null &&
                        semester.AcquiredECs < levelRequirement.EcRequirement.RequiredAmount)
                    {
                        continue;
                    }

                    matched = true;
                    checkedModule = module;
                    break;
                }

                if (!matched)
                {
                    result.GroupPassed = false;
                    result.Failures.Add((levelRequirement, false));
                }
                else
                {
                    result.GroupPassed = true;
                }
            }

            checkedModules.Add(checkedModule);
        }

        return result;
    }

    private AllGroupsResult CheckAllGroups(List<Semester> previousSemesters, List<ModuleLevelRequirementGroup> levelGroups, string moduleName)
    {
        var result = new AllGroupsResult();

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

    private class GroupResult
    {
        public List<ModuleLevelRequirement> Requirements { get; set; }
        public List<(ModuleLevelRequirement LevelRequirement, bool Missing)> Failures { get; set; } = new();
        public bool GroupPassed { get; set; }
    }

    private class AllGroupsResult
    {
        public List<GroupResult> FailureResults { get; set; } = new();
        public bool AnyGroupPassed { get; set; }
    }
}

public class ModuleRequirementRule(Func<Guid, Task<Module?>> moduleResolver) : StudyRouteValidationRuleBase
{
    private class AllGroupsResult
    {
        public List<GroupResult> FailureResults { get; set; } = new();
        public bool AnyGroupPassed { get; set; } = false;
    }

    private class GroupResult
    {
        public List<ModuleRequirement> Requirements { get; set; }
        public List<(ModuleRequirement ModuleRequirement, bool Missing)> Failures { get; set; } = new();
        public bool GroupPassed { get; set; } = true;
    }

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
                        
                        if (failureResult.Failures.Any(g => requirement.Equals(g.ModuleRequirement) && g.Missing))
                        {
                            sb.AppendLine($"- {relevantModule?.Name ?? "Module"} niet gevonden.");
                        }
                        else if (requirement.EcRequirement is not null && failureResult.Failures.Any(g => requirement.Equals(g.ModuleRequirement)))
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

    private async Task<GroupResult> CheckGroup(List<ModuleRequirement> moduleRequirements, List<Semester> relevantPreviousSemesters, string moduleName)
    {
        var result = new GroupResult();
        result.Requirements = moduleRequirements;

        foreach (var moduleRequirement in moduleRequirements)
        {
            var relevantSemester = relevantPreviousSemesters
                .FirstOrDefault(s => s.Module!.Id == moduleRequirement.RelevantModuleId);

            if (relevantSemester == null)
            {
                result.GroupPassed = false;
                result.Failures.Add((moduleRequirement, true));
                continue;
            }

            if (moduleRequirement.EcRequirement is not null &&
                relevantSemester.AcquiredECs < moduleRequirement.EcRequirement.RequiredAmount)
            {
                result.GroupPassed = false;
                result.Failures.Add((moduleRequirement, false));
            }
        }

        return result;
    }

    private async Task<AllGroupsResult> CheckAllGroups(List<Semester> previousSemesters, List<ModuleRequirementGroup> moduleRequirementGroups, string moduleName)
    {
        var result = new AllGroupsResult();

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
