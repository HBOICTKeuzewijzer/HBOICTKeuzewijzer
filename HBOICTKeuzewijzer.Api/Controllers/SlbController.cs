using HBOICTKeuzewijzer.Api.Attributes;
using HBOICTKeuzewijzer.Api.Dtos;
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
        private readonly IApplicationUserService _userService;

        public SlbController(ISlbRepository slbRepo, IApplicationUserService userService)
        {
            _slbRepo = slbRepo;
            _userService = userService;
        }

        /// <summary>
        /// Retrieves a paginated list of all SLB counselors.
        /// Accessible only to administrators.
        /// </summary>
        /// <param name="request">Pagination parameters such as page number and page size.</param>
        /// <returns>A paginated list of all SLB counselors.</returns>
        [HttpGet("list")]
        [EnumAuthorize(Role.SystemAdmin)]
        public async Task<ActionResult<PaginatedResult<Slb>>> GetPagedSlb(
            [FromQuery] GetAllRequestQuery request)
        {
            var result = await _slbRepo.GetPaginatedAsync(request, s => s.Id);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a paginated list of students linked to a speficic SLB counselor.
        /// Accessible only to administrators
        /// </summary>
        /// <param name="slbId">The ID of the SLB counselor.</param>
        /// <param name="request">Pagination parameters.</param>
        /// <returns>A paginated list of students.</returns>
        [HttpGet("{slbId:guid}/students")]
        [EnumAuthorize(Role.SystemAdmin)]
        public async Task<ActionResult<PaginatedResult<StudentDto>>> GetStudentsForSlb(Guid slbId, [FromQuery] GetAllRequestQuery request)
        {
            var result = await _slbRepo.GetStudentsBySlbAsync(slbId, request);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a paginated list of students linked to the currently logged-in SLB counselor.
        /// Accessible only to SLB users.
        /// </summary>
        /// <param name="request">Pagination parameters.</param>
        /// <returns>A paginated list of students.</returns>
        [HttpGet("myStudents")]
        [EnumAuthorize(Role.SLB)]
        public async Task<ActionResult<PaginatedResult<StudentDto>>> GetStudents(
            [FromQuery] GetAllRequestQuery request)
        {
            var currentUser = await _userService.GetOrCreateUserAsync(User);

            var result = await _slbRepo.GetStudentsBySlbAsync(currentUser.Id, request);

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

        /// <summary>
        /// Removes the relationship between an SLB counselor and a student.
        /// Accessible only to administrators.
        /// </summary>
        /// <param name="slbId">The ID of the SLB counselor.</param>
        /// <param name="studentId">The ID of the student.</param>
        /// <returns>204 NoContent on success, or 404 if the relationship was not found.</returns>
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

        /// <summary>
        /// Replaces all existing student relationships for a given SLB counselor with a new list of students.
        /// Any previously linked students not in the new list will be unlinked, and new students not yet linked will be added.
        /// Accessible only to administrators.
        /// </summary>
        /// <param name="slbId">The ID of the SLB counselor.</param>
        /// <param name="studentIds">A list of student IDs to associate with the SLB counselor.</param>
        /// <returns>204 NoContent on success.</returns>
        [HttpPut("ChangeStudents/{slbId:guid}")]
        [EnumAuthorize(Role.SystemAdmin)]
        public async Task<ActionResult> ChangeStudents(Guid slbId, [FromBody] List<Guid> studentIds)
        {
            // Get all exisiting relations
            var existingRelations = await _slbRepo.GetAllRelationsForSlbAsync(slbId);

            // Delete relations that aren't in new list
            foreach (var oldStudent in existingRelations)
            {
                if (!studentIds.Contains(oldStudent.Id))
                {
                    await _slbRepo.RemoveSlbRelationAsync(slbId, oldStudent.Id);
                }
            }

            // Add relations that didn't exist yet
            foreach (var newStudentId in studentIds)
            {
                var exists = await _slbRepo.RelationExistsAsync(slbId, newStudentId);
                if (!exists)
                {
                    await _slbRepo.AddSlbRelationAsync(slbId, newStudentId);
                }
            }

            return NoContent();
        }
    }
}
