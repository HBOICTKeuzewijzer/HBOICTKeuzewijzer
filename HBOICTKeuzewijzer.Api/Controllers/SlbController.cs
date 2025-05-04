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
        private readonly IRepository<Slb> _slbRepo;
        private readonly ApplicationUserService _userService;

        public SlbController(IRepository<Slb> slbRepo, ApplicationUserService userService)
        {
            _slbRepo = slbRepo;
            _userService = userService;
        }

        [HttpGet]
        [EnumAuthorize(Role.SLB)]
        public async Task<ActionResult<IEnumerable<ApplicationUser>>> GetStudents()
        {
            var currentUser = await _userService.GetOrCreateUserAsync(User);

            var students = await _slbRepo.Queryable()
                .Include(s => s.StudentApplicationUser)
                .Where(s => s.SlbApplicationUserId == currentUser.Id)
                .Select(s => s.StudentApplicationUser)
                .ToListAsync();

            return Ok(students);
        }

        [HttpPut("{slbId:guid}/{studentId:guid}")]
        [EnumAuthorize(Role.SystemAdmin)]
        public async Task<IActionResult> AddStudent(Guid slbId, Guid studentId)
        {
            var exists = await _slbRepo.Queryable()
                .AnyAsync(s => s.SlbApplicationUserId == slbId && s.StudentApplicationUserId == studentId);

            if (exists)
                return Conflict("Deze student is al gekoppeld aan deze SLB'er.");

            var slbRelatie = new Slb
            {
                Id = Guid.NewGuid(),
                SlbApplicationUserId = slbId,
                StudentApplicationUserId = studentId
            };

            await _slbRepo.AddAsync(slbRelatie);

            return NoContent();
        }

        [HttpDelete("{slbId:guid}/{studentId:guid}")]
        [EnumAuthorize(Role.SystemAdmin)]
        public async Task<IActionResult> RemoveStudent(Guid slbId, Guid studentId)
        {
            var relatie = await _slbRepo.Queryable()
                .FirstOrDefaultAsync(s => s.SlbApplicationUserId == slbId && s.StudentApplicationUserId == studentId);

            if (relatie == null)
                return NotFound("Relatie niet gevonden.");

            await _slbRepo.DeleteAsync(relatie.Id);

            return NoContent();
        }
    }
}
