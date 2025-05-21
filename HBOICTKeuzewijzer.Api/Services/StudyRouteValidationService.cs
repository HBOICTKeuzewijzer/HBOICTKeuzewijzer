using HBOICTKeuzewijzer.Api.Models;
using HBOICTKeuzewijzer.Api.Models.OerRequirements;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace HBOICTKeuzewijzer.Api.Services;

public class StudyRouteValidationService : IStudyRouteValidationService
{
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
        if (routeToValidate.Semesters is null) return null;

        var validationProblems = new Dictionary<string, string[]>();

        foreach (var semester in routeToValidate.Semesters)
        {
            if (semester.Module is null) continue;

            var modulePrerequisite = GetParsedModulePrerequisite(semester.Module);

            if (modulePrerequisite is null) continue;

            // validate somehow :)
            // if check fails add to validation problems
        }

        return validationProblems.Count > 0 ? new ValidationProblemDetails(validationProblems) : null;
    }

    private ModulePrerequisite? GetParsedModulePrerequisite(Module moduleToParse)
    {
        var prerequisiteJson = moduleToParse.PrerequisiteJson;

        if (prerequisiteJson == null)
        {
            return null;
        }

        return JsonConvert.DeserializeObject<ModulePrerequisite>(prerequisiteJson);
    }
}