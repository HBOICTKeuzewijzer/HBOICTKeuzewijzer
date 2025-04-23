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
    [Authorize]
    public class ModuleController : ControllerBase
    {
        private readonly IRepository<Module> _moduleRepo;
        private readonly ApplicationUserService _userService;
        

        public ModuleController(IRepository<Module> moduleRepo, ApplicationUserService userService)
        {
            _moduleRepo = moduleRepo;
            _userService = userService;
        }

        // GET: api/Module
        [HttpGet]
        public async Task<ActionResult<PaginatedResult<Module>>> GetModules(
            [FromQuery] GetAllRequestQuery request)
        {
            var result = await _moduleRepo.GetPaginatedAsync(request, m => m.Category);
            return Ok(result);
        }

        [HttpGet("count")]
        public async Task<ActionResult<int>> GetCount([FromQuery] string? filter = null)
        {
            var request = new GetAllRequestQuery { Filter = filter };
            var result = await _moduleRepo.GetPaginatedAsync(request);
            return Ok(result.TotalCount);
        }

        // GET: api/Module/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Module>> GetModule(Guid id)
        {
            var module = await _moduleRepo.GetByIdAsync(id);

            if (module == null)
            {
                return NotFound();
            }

            return module;
        }

        // PUT: api/Module/5
        [HttpPut("{id}")]
        [EnumAuthorize(Role.ModuleAdmin, Role.SystemAdmin)]
        public async Task<IActionResult> PutModule(Guid id, Module module)
        {
            if (id != module.Id)
            {
                return BadRequest("Id en url komt niet overeen met de module ID");
            }

            if (!await _moduleRepo.ExistsAsync(id))
            {
                return NotFound();
            }

            var user = await _userService.GetOrCreateUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            try
            {
                await _moduleRepo.UpdateAsync(module);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _moduleRepo.ExistsAsync(id))
                {
                    return NotFound();
                }

                throw;
            }

            return NoContent();
        }

        // POST: api/Module
        [HttpPost]
        [EnumAuthorize(Role.ModuleAdmin, Role.SystemAdmin)]
        public async Task<ActionResult<Module>> PostModule(Module module)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userService.GetOrCreateUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            await _moduleRepo.AddAsync(module);

            return CreatedAtAction(nameof(GetModule), new { id = module.Id }, module);
        }

        // DELETE: api/Module/5
        [HttpDelete("{id}")]
        [EnumAuthorize(Role.SystemAdmin, Role.ModuleAdmin)]
        public async Task<IActionResult> DeleteModule(Guid id)
        {
            var module = await _moduleRepo.GetByIdAsync(id);
            if (module == null)
            {
                return NotFound();
            }

            await _moduleRepo.DeleteAsync(id);

            return NoContent();
        }
    }
}
