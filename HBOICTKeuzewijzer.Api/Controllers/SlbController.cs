using HBOICTKeuzewijzer.Api.Attributes;
using HBOICTKeuzewijzer.Api.Models;
using HBOICTKeuzewijzer.Api.Repositories;
using HBOICTKeuzewijzer.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HBOICTKeuzewijzer.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SlbController : ControllerBase
    {
        private readonly ISlbRepository _slbRepo;
        private readonly ApplicationUserService _userService;

        public SlbController(ISlbRepository slbRepo, ApplicationUserService userService)
        {
            _slbRepo = slbRepo;
            _userService = userService;
        }

        [HttpGet]
        [EnumAuthorize(Role.SLB)]
        public async Task<ActionResult<PaginatedResult<ApplicationUser>>> GetStudents(
            [FromQuery] GetAllRequestQuery request)
        {
            var currentUser = await _userService.GetOrCreateUserAsync(User);

            var result = await _slbRepo.GetStudentsBySlbAsync(currentUser.Id, request.Page, request.PageSize);

            return Ok(result);
        }

        [HttpPut("{slbId:guid}/{studentId:guid}")]
        [EnumAuthorize(Role.SystemAdmin)]
        public async Task<IActionResult> AddStudent(Guid slbId, Guid studentId)
        {
            try
            {
                await _slbRepo.AddSlbRelationAsync(slbId, studentId);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{slbId:guid}/{studentId:guid}")]
        [EnumAuthorize(Role.SystemAdmin)]
        public async Task<IActionResult> RemoveStudent(Guid slbId, Guid studentId)
        {
            try
            {
                await _slbRepo.RemoveSlbRelationAsync(slbId, studentId);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
