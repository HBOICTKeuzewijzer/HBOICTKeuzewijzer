using HBOICTKeuzewijzer.Api.Attributes;
using HBOICTKeuzewijzer.Api.Models;
using HBOICTKeuzewijzer.Api.Repositories;
using HBOICTKeuzewijzer.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HBOICTKeuzewijzer.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class OerController : ControllerBase
    {
        private readonly IRepository<Oer> _oerRepo;
        private readonly IApplicationUserService _userService;
        private readonly IOerUploadService _oerUploadService;

        public OerController(IRepository<Oer> oerRepo, IApplicationUserService userService, IOerUploadService oerUploadService)
        {
            _oerRepo = oerRepo;
            _userService = userService;
            _oerUploadService = oerUploadService;
        }

        // GET: /oer/
        [HttpGet]
        public async Task<ActionResult<PaginatedResult<Oer>>> GetPagedOer(
            [FromQuery] GetAllRequestQuery request)
        {
            var result = await _oerRepo.GetPaginatedAsync(request, o => o.Modules);
            return Ok(result);
        }

        // GET: /oer/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Oer>> GetOer(Guid id)
        {
            var oer = await _oerRepo.GetByIdAsync(id);

            if (oer == null)
            {
                return NotFound();
            }

            return oer;
        }

        // POST: /oer/
        [HttpPost]
        [EnumAuthorize(Role.ModuleAdmin, Role.SystemAdmin)]
        public async Task<ActionResult<Oer>> PostOer(Oer oer)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userService.GetOrCreateUserAsync(User);

            await _oerRepo.AddAsync(oer);

            return CreatedAtAction(nameof(GetOer), new { id = oer.Id }, oer);
        }

        // POST: /oer/id/upload
        [HttpPost("{id}/upload")]
        [EnumAuthorize(Role.ModuleAdmin, Role.SystemAdmin)]
        public async Task<IActionResult> UploadPdf(Guid id, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Geen bestand ontvangen");

            if (!file.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Alleen PDF-bestanden zijn toegestaan");

            var oer = await _oerRepo.GetByIdAsync(id);
            if (oer == null)
                return NotFound();

            var fileUrl = await _oerUploadService.SavePdfAsync(oer, file);
            oer.Filepath = fileUrl;
            await _oerRepo.UpdateAsync(oer);

            return Ok(new { fileUrl });
        }

        // PUT: /oer/5
        [HttpPut("{id}")]
        [EnumAuthorize(Role.ModuleAdmin, Role.SystemAdmin)]
        public async Task<IActionResult> PutOer(Guid id, Oer oer)
        {
            if (id != oer.Id)
            {
                return BadRequest("Id en url komt niet overeen met het OER ID");
            }

            if (!await _oerRepo.ExistsAsync(id))
            {
                return NotFound();
            }

            var user = await _userService.GetOrCreateUserAsync(User);

            try
            {
                await _oerRepo.UpdateAsync(oer);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _oerRepo.ExistsAsync(id))
                {
                    return NotFound();
                }

                throw;
            }

            return CreatedAtAction(nameof(GetOer), new { id = oer.Id }, oer);
        }

        // DELETE: /oer/5
        [HttpDelete("{id}")]
        [EnumAuthorize(Role.ModuleAdmin, Role.SystemAdmin)]
        public async Task<IActionResult> DeleteOer(Guid id)
        {
            var oer = await _oerRepo.GetByIdAsync(id);
            if (oer == null)
            {
                return NotFound();
            }

            await _oerRepo.DeleteAsync(id);

            return NoContent();
        }
    }
}
