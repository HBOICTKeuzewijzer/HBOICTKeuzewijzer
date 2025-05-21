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

        /// <summary>
        /// Haalt een paginated lijst van alle SLB'ers op.
        /// Alleen toegankelijk voor beheerders.
        /// </summary>
        /// <param name="request">Paginatieparameters zoals pagina en grootte.</param>
        /// <returns>Een paginated lijst van SLB'ers.</returns>
        [HttpGet("list")]
        [EnumAuthorize(Role.SystemAdmin)]
        public async Task<ActionResult<PaginatedResult<Slb>>> GetPagedSlb(
            [FromQuery] GetAllRequestQuery request)
        {
            var result = await _slbRepo.GetPaginatedAsync(request, s => s.Id);
            return Ok(result);
        }

        /// <summary>
        /// Haalt een paginated lijst van studenten op die gekoppeld zijn aan een specifieke SLB'er.
        /// Alleen toegankelijk voor beheerders.
        /// </summary>
        /// <param name="slbId">De ID van de SLB'er.</param>
        /// <param name="request">Paginatieparameters.</param>
        /// <returns>Een paginated lijst van studenten.</returns>
        [HttpGet("{slbId:guid}/students")]
        [EnumAuthorize(Role.SystemAdmin)]
        public async Task<ActionResult<PaginatedResult<ApplicationUser>>> GetStudentsForSlb(Guid slbId, [FromQuery] GetAllRequestQuery request)
        {
            var result = await _slbRepo.GetStudentsBySlbAsync(slbId, request);
            return Ok(result);
        }

        /// <summary>
        /// Haalt een paginated lijst van studenten op die gekoppeld zijn aan de ingelogde SLB'er.
        /// Alleen toegankelijk voor SLB-gebruikers.
        /// </summary>
        /// <param name="request">Paginatieparameters.</param>
        /// <returns>Een paginated lijst van studenten.</returns>
        [HttpGet("myStudents")]
        [EnumAuthorize(Role.SLB)]
        public async Task<ActionResult<PaginatedResult<ApplicationUser>>> GetStudents(
            [FromQuery] GetAllRequestQuery request)
        {
            var currentUser = await _userService.GetOrCreateUserAsync(User);

            var result = await _slbRepo.GetStudentsBySlbAsync(currentUser.Id, request);

            return Ok(result);
        }

        /// <summary>
        /// Voegt een relatie toe tussen een SLB'er en een student.
        /// Alleen toegankelijk voor beheerders.
        /// </summary>
        /// <param name="slbId">De ID van de SLB'er.</param>
        /// <param name="studentId">De ID van de student.</param>
        /// <returns>204 NoContent bij succes, of een foutmelding bij failure.</returns>
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
        /// Verwijdert de relatie tussen een SLB'er en een student.
        /// Alleen toegankelijk voor beheerders.
        /// </summary>
        /// <param name="slbId">De ID van de SLB'er.</param>
        /// <param name="studentId">De ID van de student.</param>
        /// <returns>204 NoContent bij succes, of 404 bij niet gevonden relatie.</returns>
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
