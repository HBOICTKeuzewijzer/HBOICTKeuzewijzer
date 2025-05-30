using HBOICTKeuzewijzer.Api.Attributes;
using HBOICTKeuzewijzer.Api.Models;
using HBOICTKeuzewijzer.Api.Repositories;
using HBOICTKeuzewijzer.Api.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HBOICTKeuzewijzer.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class StudyRouteController : ControllerBase
    {
        private readonly IStudyRouteRepository _studyRouteRepository;
        private readonly IApplicationUserService _applicationUserService;
        private readonly IModuleRepository _moduleRepository;

        public StudyRouteController(IStudyRouteRepository studyRouteRepository, IModuleRepository moduleRepository, IApplicationUserService applicationUserService)
        {
            _studyRouteRepository = studyRouteRepository;
            _applicationUserService = applicationUserService;
            _moduleRepository = moduleRepository;
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

            if (await _studyRouteRepository.DeleteForUser(id, student))
            {
                return NoContent();
            }

            return NotFound("Study route not found or not authorized to delete.");
        }

        [EnumAuthorize(Role.Student)]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStudyroute(Guid id, [FromBody] StudyRoute updatedRoute)
        {
            var student = await GetUser;
            if (student is null)
                return BadRequest("No user found.");

            if (updatedRoute.Semesters is null)
                return BadRequest();

            var existingRoute = await _studyRouteRepository.GetForUserById(student, id);
            if (existingRoute is null)
                return NotFound("Study route not found or not authorized to update.");

            foreach (var semester in existingRoute.Semesters!)
            {
                var relevantSemester = updatedRoute.Semesters.FirstOrDefault(s => s.Id == semester.Id);

                semester.ModuleId = relevantSemester?.ModuleId ?? null;
            }

            await _studyRouteRepository.UpdateAsync(existingRoute);

            return Ok(existingRoute);
        }



        private Task<ApplicationUser?> GetUser => _applicationUserService.GetByPrincipal(User);
    }
}

