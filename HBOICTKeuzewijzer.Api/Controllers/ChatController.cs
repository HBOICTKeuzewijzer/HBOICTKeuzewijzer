using HBOICTKeuzewijzer.Api.Models;
using HBOICTKeuzewijzer.Api.Repositories;
using HBOICTKeuzewijzer.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace HBOICTKeuzewijzer.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IRepository<Chat> _chatRepository;
        private readonly ApplicationUserService _userService;

        public ChatController(IRepository<Chat> chatRepository, ApplicationUserService userService)
        {
            _chatRepository = chatRepository;
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResult<Chat>>> List([FromQuery] GetAllRequestQuery request)
        {
            var result = await _chatRepository.GetPaginatedAsync(request);

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Chat>> Read(Guid id)
        {
            // Check if the chat exists.
            var chat = await _chatRepository.GetByIdAsync(id);
            if (chat == null)
                return NotFound();

            return Ok(chat);
        }

        [HttpPost]
        public async Task<ActionResult<Chat>> Create([FromBody] Chat chat)
        {
            await _chatRepository.AddAsync(chat);

            return CreatedAtAction(nameof(Read), new { id = chat.Id }, chat);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            // Check if the chat exists.
            var existing = await _chatRepository.GetByIdAsync(id);
            if (existing == null)
                return NotFound();

            await _chatRepository.DeleteAsync(id);
            
            return NoContent();
        }
    }
}
