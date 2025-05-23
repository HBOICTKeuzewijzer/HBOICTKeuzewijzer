using HBOICTKeuzewijzer.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace HBOICTKeuzewijzer.Api.Services.StudyRouteValidation;

public interface IStudyRouteValidationService
{
    ValidationProblemDetails? ValidateRoute(StudyRoute routeToValidate);
}