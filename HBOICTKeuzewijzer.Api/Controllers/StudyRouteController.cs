using HBOICTKeuzewijzer.Api.Attributes;
using HBOICTKeuzewijzer.Api.Models;
using HBOICTKeuzewijzer.Api.Repositories;
using HBOICTKeuzewijzer.Api.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using HBOICTKeuzewijzer.Api.Services.StudyRouteValidation;

namespace HBOICTKeuzewijzer.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class StudyRouteController : ControllerBase
    {
        private readonly IStudyRouteRepository _studyRouteRepository;
        private readonly IApplicationUserService _applicationUserService;
        private readonly IModuleRepository _moduleRepository;
        private readonly IRepository<CustomModule> _customModuleRepository;
        private readonly IStudyRouteValidationService _studyRouteValidationService;

        public StudyRouteController(IStudyRouteRepository studyRouteRepository, IModuleRepository moduleRepository, IApplicationUserService applicationUserService, IRepository<CustomModule> customModuleRepository, IStudyRouteValidationService studyRouteValidationService)
        {
            _studyRouteRepository = studyRouteRepository;
            _applicationUserService = applicationUserService;
            _moduleRepository = moduleRepository;
            _customModuleRepository = customModuleRepository;
            _studyRouteValidationService = studyRouteValidationService;
        }

        [EnumAuthorize(Role.Student)]
        [HttpGet("mine")]
        public async Task<ActionResult> GetStudyRoutes()
        {
            var student = await GetUser;

            if (student is null)
            {
                return BadRequest("No user found.");
            }

            var data = await _studyRouteRepository.GetForUser(student);

            return Ok(new PaginatedResult<StudyRoute>
            {
                Items = data,
                TotalCount = data.Count()
            });
        }

        [EnumAuthorize(Role.Student, Role.SLB)]
        [HttpGet("{id}")]
        public async Task<ActionResult> GetStudyRoute(Guid id)
        {
            var user = await GetUser;

            if (user is null)
            {
                return BadRequest("No user found.");
            }

            StudyRoute? route;

            if (User.IsInRole(Role.SLB.ToString()))
            {
                // SLB can access any route
                route = await _studyRouteRepository.GetByIdWithSemesters(id);
            }
            else
            {
                // Students can only access their own
                route = await _studyRouteRepository.GetForUserById(user, id);
            }

            if (route is null)
            {
                return NotFound("Study route not found.");
            }

            return Ok(route);
        }

        [EnumAuthorize(Role.Student)]
        [HttpPost]
        public async Task<ActionResult> AddStudyRoute([FromQuery] string displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName) || displayName.Length > 255)
            {
                var problemDetails = new ValidationProblemDetails(new Dictionary<string, string[]>
                {
                    { "displayName", ["Naam moet minimaal 1 en maximaal 255 characters bevatten."] }
                });

                return ValidationProblem(problemDetails);
            }

            var student = await GetUser;

            if (student is null)
            {
                return BadRequest("No user found.");
            }

            var all = await _studyRouteRepository.GetForUser(student);

            if (all.Count == 10)
            {
                var problemDetails = new ValidationProblemDetails(new Dictionary<string, string[]>
                {
                    { "displayName", ["Je hebt het maximale aantal routes van 10 bereikt."] }
                });

                return ValidationProblem(problemDetails);
            }

            var newRoute = await _studyRouteRepository.AddWithUniqueDisplayName(student, displayName);

            await _moduleRepository.FillWithRequiredModules(newRoute);

            return CreatedAtAction(nameof(GetStudyRoute), new { id = newRoute.Id }, newRoute);
        }

        [EnumAuthorize(Role.Student)]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteStudyRoute(Guid id)
        {
            var student = await GetUser;

            if (student is null)
            {
                return BadRequest("No user found.");
            }

            var existingRoute = await _studyRouteRepository.GetForUserById(student, id);
            if (existingRoute is null)
            {
                return NotFound("Study route not found or not authorized to delete.");
            }

            foreach (var semester in existingRoute.Semesters!)
            {
                if (semester.CustomModuleId is not null)
                {
                    await _customModuleRepository.DeleteAsync(semester.CustomModuleId.Value);
                }
            }

            await _studyRouteRepository.DeleteAsync(existingRoute.Id);
            
            return NoContent();
        }

        [EnumAuthorize(Role.Student)]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStudyroute(Guid id, [FromBody] StudyRoute updatedRoute)
        {
            var student = await GetUser;
            if (student is null) return BadRequest("No user found.");
            if (updatedRoute.Semesters is null) return BadRequest();

            var existingRoute = await _studyRouteRepository.GetForUserById(student, id);
            if (existingRoute is null) return NotFound("Study route not found or not authorized to update.");

            foreach (var semester in existingRoute.Semesters!)
            {
                var updatedSemester = updatedRoute.Semesters.FirstOrDefault(s => s.Id == semester.Id);
                if (updatedSemester is null) continue;

                await UpdateSemester(semester, updatedSemester);
            }

            await _studyRouteRepository.UpdateAsync(existingRoute);

            var validationResult = await _studyRouteValidationService.ValidateRoute(existingRoute);
            if (validationResult is not null)
            {
                return ValidationProblem(validationResult);
            }

            return Ok(existingRoute);
        }

        private async Task UpdateSemester(Semester semester, Semester updatedSemester)
        {
            // Handle custom module cleanup first
            if (semester.CustomModule is not null)
            {
                await _customModuleRepository.DeleteAsync(semester.CustomModule.Id);
            }

            // Determine which module type we're dealing with
            if (updatedSemester.ModuleId is not null)
            {
                if (await ApplyNormalModule(semester, updatedSemester.ModuleId.Value))
                {
                    semester.AcquiredECs = 0;
                    return;
                }
            }
            else if (updatedSemester.CustomModule is not null)
            {
                ApplyCustomModule(semester, updatedSemester.CustomModule);
            }
            
            // Only update AcquiredECs after module switching logic is handled
            semester.AcquiredECs = updatedSemester.AcquiredECs;
        }

        private async Task<bool> ApplyNormalModule(Semester semester, Guid moduleId)
        {
            if (semester.ModuleId != moduleId)
            {
                semester.ModuleId = moduleId;
                semester.Module = await _moduleRepository.GetByIdAsync(moduleId);
                return true;
            }

            return false;
        }

        private void ApplyCustomModule(Semester semester, CustomModule customModule)
        {
            semester.CustomModule = customModule;
            semester.CustomModuleId = customModule.Id;
            semester.ModuleId = null;
        }

        private Task<ApplicationUser?> GetUser => _applicationUserService.GetByPrincipal(User);
    }
}

