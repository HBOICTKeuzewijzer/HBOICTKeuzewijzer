using HBOICTKeuzewijzer.Api.Models;
using HBOICTKeuzewijzer.Api.Models.OerRequirements;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Drawing;
using HBOICTKeuzewijzer.Api.Repositories;

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
            new ModuleRequirementRule(ModuleResolver)
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

        if (modulePrerequisite?.ModuleLevelRequirementGroups == null || modulePrerequisite.ModuleLevelRequirementGroups.Count == 0) return Task.CompletedTask;

        foreach (var moduleLevelRequirementGroup in modulePrerequisite.ModuleLevelRequirementGroups)
        {
            if (moduleLevelRequirementGroup.ModuleLevelRequirements.Count == 0) continue;

            foreach (var moduleLevelRequirement in moduleLevelRequirementGroup.ModuleLevelRequirements)
            {
                
            }
        }

        return Task.CompletedTask;
    }
}

public class ModuleRequirementRule(Func<Guid, Task<Module?>> moduleResolver) : StudyRouteValidationRuleBase
{
    public override async Task Validate(Semester currentSemester, List<Semester> previousSemesters, Dictionary<string, List<string>> errors)
    {
        if (!GetParsedPrerequisite(currentSemester, out var modulePrerequisite)) return;

        if (modulePrerequisite?.ModuleRequirementGroups == null || modulePrerequisite.ModuleRequirementGroups.Count == 0) return;

        var moduleName = currentSemester.Module!.Name;
        var semesterId = currentSemester.Id.ToString();

        var semestersWithModules = previousSemesters.Where(s => s.Module is not null).ToList();

        bool anyGroupPassed = false;
        List<string> collectedErrors = new();

        foreach (var moduleRequirementGroup in modulePrerequisite.ModuleRequirementGroups)
        {
            if (moduleRequirementGroup.ModuleRequirements.Count == 0) continue;

            bool groupPassed = true;
            List<string> groupErrors = new();

            foreach (var moduleRequirement in moduleRequirementGroup.ModuleRequirements)
            {
                var relevantModule = await moduleResolver(moduleRequirement.RelevantModuleId);

                var relevantSemester = semestersWithModules
                    .FirstOrDefault(s => s.Module!.Id == moduleRequirement.RelevantModuleId);

                if (relevantSemester == null)
                {
                    groupPassed = false;
                    groupErrors.Add(
                        $"Module: {moduleName} vereist dat module '{relevantModule?.Name}' aanwezig is in een voorgaand semester, maar deze module is niet gevonden.");
                    continue;
                }

                if (moduleRequirement.EcRequirement is not null &&
                    relevantSemester.AcquiredECs < moduleRequirement.EcRequirement.RequiredAmount)
                {
                    groupPassed = false;
                    groupErrors.Add(
                        $"Module: {moduleName} verwacht dat uit module '{relevantModule?.Name}' minimaal {moduleRequirement.EcRequirement.RequiredAmount} ec zijn behaald, huidige behaalde ec's is {relevantSemester.AcquiredECs}");
                }
            }

            if (groupPassed)
            {
                anyGroupPassed = true;
                break;
            }

            collectedErrors.AddRange(groupErrors); 
        }

        if (!anyGroupPassed)
        {
            foreach (var error in collectedErrors)
            {
                AddError(error, semesterId, errors);
            }
        }
    }

}
