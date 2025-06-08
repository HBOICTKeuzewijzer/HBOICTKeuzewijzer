using HBOICTKeuzewijzer.Api.Attributes;
using HBOICTKeuzewijzer.Api.Models;
using HBOICTKeuzewijzer.Api.Repositories;
using HBOICTKeuzewijzer.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HBOICTKeuzewijzer.Api.Controllers;

[Route("[controller]")]
[ApiController]
public class ModuleController : ControllerBase
{
    private readonly IRepository<Module> _moduleRepo;
    public ModuleController(IRepository<Module> moduleRepo)
    {
        _moduleRepo = moduleRepo;
    }

    // GET: Module/paged
    [HttpGet("paged")]
    public async Task<ActionResult<PaginatedResult<Module>>> GetPagedModules(
        [FromQuery] GetAllRequestQuery request)
    {
        var result = await _moduleRepo.GetPaginatedAsync(request, m => m.Category);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<List<Module>>> GetModules()
    {
        var result = await _moduleRepo.GetAllIncludingAsync(m => m.Category);

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

        if (module == null) return NotFound();

        return module;
    }

    // PUT: api/Module/5
    [HttpPut("{id}")]
    [EnumAuthorize(Role.ModuleAdmin, Role.SystemAdmin)]
    public async Task<IActionResult> PutModule(Guid id, Module module)
    {
        if (id != module.Id) return BadRequest("Id en url komt niet overeen met de module ID");

        if (!await _moduleRepo.ExistsAsync(id)) return NotFound();

        try
        {
            await _moduleRepo.UpdateAsync(module);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _moduleRepo.ExistsAsync(id)) return NotFound();

            throw;
        }

        return NoContent();
    }

    // POST: api/Module
    [HttpPost]
    [EnumAuthorize(Role.ModuleAdmin, Role.SystemAdmin)]
    public async Task<ActionResult<Module>> PostModule(Module module)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        await _moduleRepo.AddAsync(module);

        return CreatedAtAction(nameof(GetModule), new { id = module.Id }, module);
    }

    // DELETE: api/Module/5
    [HttpDelete("{id}")]
    [EnumAuthorize(Role.SystemAdmin, Role.ModuleAdmin)]
    public async Task<IActionResult> DeleteModule(Guid id)
    {
        var module = await _moduleRepo.GetByIdAsync(id);
        if (module == null) return NotFound();

        await _moduleRepo.DeleteAsync(id);

        return NoContent();
    }
}