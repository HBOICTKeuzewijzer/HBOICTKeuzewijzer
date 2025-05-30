using HBOICTKeuzewijzer.Api.Attributes;
using HBOICTKeuzewijzer.Api.Models;
using HBOICTKeuzewijzer.Api.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace HBOICTKeuzewijzer.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly IRepository<Category> _categoryRepo;

        public CategoryController(IRepository<Category> categoryRepo)
        {
            _categoryRepo = categoryRepo;
        }

        // GET: api/Category, test(bas)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            var categories = await _categoryRepo.GetAllAsync();

            var ordered = categories.OrderBy(c => c.Position);

            return Ok(ordered);
        }

        // GET: api/Category/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategory(Guid id)
        {
            var category = await _categoryRepo.GetByIdAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            return Ok(category);
        }

        // PUT: api/Category/5
        [HttpPut("{id}")]
        [EnumAuthorize(Role.SystemAdmin, Role.ModuleAdmin)]
        public async Task<IActionResult> PutCategory(Guid id, Category category)
        {
            if (id != category.Id)
            {
                return BadRequest();
            }

            category.Modules = null; // met dit

            await _categoryRepo.UpdateAsync(category);
            return NoContent();
        }

        // POST: api/Category
        [HttpPost]
        [EnumAuthorize(Role.SystemAdmin, Role.ModuleAdmin)]
        public async Task<ActionResult<Category>> PostCategory(Category category)
        {
            await _categoryRepo.AddAsync(category);

            return CreatedAtAction("GetCategory", new { id = category.Id }, category);
        }
        
        [HttpGet("paginated")]
        [EnumAuthorize(Role.SystemAdmin, Role.ModuleAdmin)]
        public async Task<ActionResult<PaginatedResult<Category>>> GetPaginatedCategories([FromQuery] GetAllRequestQuery request)
        {
            var result = await _categoryRepo.GetPaginatedAsync(request);
            return Ok(result);
        }

        // DELETE: api/Category/5
        [HttpDelete("{id}")]
        [EnumAuthorize(Role.SystemAdmin, Role.ModuleAdmin)]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            var category = await _categoryRepo.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            await _categoryRepo.DeleteAsync(id);
            return NoContent();
        }
    }
}