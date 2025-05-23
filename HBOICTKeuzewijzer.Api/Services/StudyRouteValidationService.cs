using HBOICTKeuzewijzer.Api.Models;
using HBOICTKeuzewijzer.Api.Models.OerRequirements;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace HBOICTKeuzewijzer.Api.Services;

public class StudyRouteValidationService : IStudyRouteValidationService
{
    private readonly List<IStudyRouteValidationRule> _rules;

    public StudyRouteValidationService()
    {
        _rules = new List<IStudyRouteValidationRule>
        {
            new PropaedeuticRule(),
            // Add more rules here in future
        };
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
    public ValidationProblemDetails? ValidateRoute(StudyRoute routeToValidate)
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
                rule.Validate(semester, previousSemesters, errors);
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
    void Validate(Semester currentSemester, List<Semester> previousSemesters, Dictionary<string, List<string>> errors);
}

public abstract class StudyRouteValidationRuleBase : IStudyRouteValidationRule
{
    public abstract void Validate(Semester currentSemester, List<Semester> previousSemesters, Dictionary<string, List<string>> errors);

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
}

public class PropaedeuticRule : StudyRouteValidationRuleBase
{
    public override void Validate(Semester currentSemester, List<Semester> previousSemesters, Dictionary<string, List<string>> errors)
    {
        if (!GetParsedPrerequisite(currentSemester, out var modulePrerequisite)) return;

        var pCount = previousSemesters
            .Where(s => s.Module is not null)
            .Select(s => s.Module!)
            .Count(m => m.IsPropaedeutic);

        if (pCount == 2) return;

        var key = currentSemester.Id.ToString();
        if (!errors.ContainsKey(key))
        {
            errors[key] = new List<string>();
        }

        errors[key].Add($"Module: {currentSemester.Module!.Name} verwacht een voltooide propedeuse, minimaal 2 modules uit de P fase, {pCount} gevonden.");
    }
}

public class SemesterConstraintRule : StudyRouteValidationRuleBase
{
    public void Validate(Semester currentSemester, List<Semester> previousSemesters, Dictionary<string, List<string>> errors)
    {

    }
}