using HBOICTKeuzewijzer.Api.Attributes;
using HBOICTKeuzewijzer.Api.Models;
using HBOICTKeuzewijzer.Api.Repositories;
using HBOICTKeuzewijzer.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace HBOICTKeuzewijzer.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class StudyRouteController : ControllerBase
    {
        private readonly IStudyRouteRepository _studyRouteRepository;
        private readonly ApplicationUserService _applicationUserService;

        public StudyRouteController(IStudyRouteRepository studyRouteRepository, ApplicationUserService applicationUserService)
        {
            _studyRouteRepository = studyRouteRepository;
            _applicationUserService = applicationUserService;
        }

        [EnumAuthorize(Role.Student)]
        [HttpGet("mine")]
        public async Task<ActionResult<PaginatedResult<StudyRoute>>> GetStudyRoutes()
        {
            var student = await _applicationUserService.GetByPrincipal(User);

            if (student == null)
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

        [EnumAuthorize(Role.Student)]
        [HttpPost]
        public async Task<ActionResult> AddStudyRoute([FromQuery] string displayName)
        {
            if (displayName.Length < 1 && displayName.Length > 100)
                return BadRequest("Displayname may not be smaller than 1 or bigger than 100 characters.");

            var student = await _applicationUserService.GetByPrincipal(User);

            if (student == null)
            {
                return BadRequest("No user found.");
            }

            await _studyRouteRepository.AddAsync(new StudyRoute
            {
                ApplicationUserId = student.Id,
                DisplayName = displayName
            });

            return Ok();
        }
    }
}

