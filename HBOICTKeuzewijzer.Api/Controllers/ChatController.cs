using HBOICTKeuzewijzer.Api.Models;
using HBOICTKeuzewijzer.Api.Repositories;
using HBOICTKeuzewijzer.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        private async Task<(ApplicationUser user, Chat? chat)> GetAuthorizedChat(Guid chatId)
        {
            var user = await _userService.GetOrCreateUserAsync(User);
            var chat = await _chatRepository.GetByIdAsync(chatId);

            if (chat == null ||
                (chat.SlbApplicationUserId != user.Id && chat.StudentApplicationUserId != user.Id))
            {
                return (user, null);
            }

            return (user, chat);
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResult<Chat>>> List([FromQuery] GetAllRequestQuery request)
        {
            var user = await _userService.GetOrCreateUserAsync(User);

            // Handmatig query opbouwen
            var query = _chatRepository
                .Query() // dit moet je toevoegen aan IRepository<T>
                .Where(c => c.SlbApplicationUserId == user.Id || c.StudentApplicationUserId == user.Id)
                .Include(c => c.Messages); // <-- mag hier wél veilig

            // Paginate zelf
            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((request.Page.GetValueOrDefault(1) - 1) * request.PageSize.GetValueOrDefault(10))
                .Take(request.PageSize.GetValueOrDefault(10))
                .ToListAsync();

            return Ok(new PaginatedResult<Chat>
            {
                Items = items,
                TotalCount = totalCount,
                Page = request.Page ?? 1,
                PageSize = request.PageSize ?? totalCount
            });
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<Chat>> Read(Guid id)
        {
            var (user, chat) = await GetAuthorizedChat(id);
            if (chat == null) return NotFound();

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
            var (user, chat) = await GetAuthorizedChat(id);
            if (chat == null) return NotFound();

            await _chatRepository.DeleteAsync(id);
            return NoContent();
        }

    }
}
