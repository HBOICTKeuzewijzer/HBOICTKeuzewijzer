using HBOICTKeuzewijzer.Api.Models;
using HBOICTKeuzewijzer.Api.Dtos;
using HBOICTKeuzewijzer.Api.Repositories;
using HBOICTKeuzewijzer.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace HBOICTKeuzewijzer.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ModuleReviewController : ControllerBase
    {
        private readonly IRepository<ModuleReview> _reviewRepo;
        private readonly IApplicationUserService _userService;

        public ModuleReviewController(IRepository<ModuleReview> reviewRepo, IApplicationUserService userService)
        {
            _reviewRepo = reviewRepo;
            _userService = userService;
        }

        // GET: /ModuleReview/{moduleId}
        [HttpGet("{moduleId}")]
        public async Task<ActionResult<List<ModuleReviewResponseDto>>> GetReviews(Guid moduleId)
        {
            var reviews = await _reviewRepo.Query()
                .Include(r => r.Student)
                .Where(r => r.ModuleId == moduleId)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ModuleReviewResponseDto
                {
                    StudentName = r.Student.DisplayName,
                    ReviewText = r.ReviewText,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            return Ok(reviews);
        }

        // POST: /ModuleReview
        [HttpPost]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult> PostReview([FromBody] ModuleReviewDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userService.GetOrCreateUserAsync(User);

            var review = new ModuleReview
            {
                ModuleId = dto.ModuleId,
                ReviewText = dto.ReviewText,
                StudentId = user.Id
            };

            await _reviewRepo.AddAsync(review);

            return Ok();
        }
    }
}
