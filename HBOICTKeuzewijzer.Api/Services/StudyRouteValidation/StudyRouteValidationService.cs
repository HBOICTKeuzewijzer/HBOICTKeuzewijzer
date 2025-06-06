using HBOICTKeuzewijzer.Api.Models;
using HBOICTKeuzewijzer.Api.Repositories;
using HBOICTKeuzewijzer.Api.Services.StudyRouteValidation.Validators;
using Microsoft.AspNetCore.Mvc;

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
            new SemesterConstraintRule(),
            new EcRequirementRule(),
            new YearRequirementRule(),
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
            Status = 200,
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
