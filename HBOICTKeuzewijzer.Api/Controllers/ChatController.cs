using HBOICTKeuzewijzer.Api.Models;
using HBOICTKeuzewijzer.Api.Repositories;
using HBOICTKeuzewijzer.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HBOICTKeuzewijzer.Api.Dtos;


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
        [HttpGet]
        public async Task<ActionResult<PaginatedResult<Chat>>> List([FromQuery] GetAllRequestQuery request)
        {
            var user = await _userService.GetOrCreateUserAsync(User);

            // Include SLB and Student in the query
            var query = _chatRepository
                .Query()
                .Where(c => c.SlbApplicationUserId == user.Id || c.StudentApplicationUserId == user.Id)
                .Include(c => c.Messages)
                .Include(c => c.SLB) // Include SLB user details
                .Include(c => c.Student); // Include Student user details

            // Paginate the results
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

            // Include SLB and Student details in the response
            chat = await _chatRepository
                .Query()
                .Include(c => c.SLB) // Include SLB user details
                .Include(c => c.Student) // Include Student user details
                .FirstOrDefaultAsync(c => c.Id == id);

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

        [HttpGet("has-unread")]
        public async Task<ActionResult<List<ChatUnreadDto>>> HasUnreadMessages()
        {
            var user = await _userService.GetOrCreateUserAsync(User);

            var isSlb = await _chatRepository.Query().AnyAsync(c => c.SlbApplicationUserId == user.Id);
            var isStudent = await _chatRepository.Query().AnyAsync(c => c.StudentApplicationUserId == user.Id);

            if (!isSlb && !isStudent)
            {
                return Ok(new List<ChatUnreadDto>());
            }

            var chats = await _chatRepository.Query()
                .Where(c => (isSlb && c.SlbApplicationUserId == user.Id) ||
                            (isStudent && c.StudentApplicationUserId == user.Id))
                .Select(chat => new ChatUnreadDto
                {
                    ChatId = chat.Id,
                    HasUnread = isSlb
                        ? chat.Messages.Any(m => !m.SlbRead)
                        : chat.Messages.Any(m => !m.StudentRead)
                })
                .ToListAsync();

            return Ok(chats);
        }

        [HttpPut("mark-as-read/{chatId}")]
        public async Task<IActionResult> MarkChatAsRead(Guid chatId)
        {
            var user = await _userService.GetOrCreateUserAsync(User);

            var isSlb = await _chatRepository.Query().AnyAsync(c => c.SlbApplicationUserId == user.Id);
            var isStudent = await _chatRepository.Query().AnyAsync(c => c.StudentApplicationUserId == user.Id);

            if (!isSlb && !isStudent)
            {
                return BadRequest("De gebruiker heeft geen toegang tot deze chat.");
            }

            var chat = await _chatRepository.Query()
                .Include(c => c.Messages) 
                .Where(c => c.Id == chatId &&
                            ((isSlb && c.SlbApplicationUserId == user.Id) ||
                             (isStudent && c.StudentApplicationUserId == user.Id)))
                .FirstOrDefaultAsync();

            if (chat == null)
            {
                return NotFound("Chat niet gevonden.");
            }

            if (isSlb)
            {
                chat.Messages
                    .Where(m => !m.SlbRead)
                    .ToList()
                    .ForEach(m => m.SlbRead = true);
            }
            else if (isStudent)
            {
                chat.Messages
                    .Where(m => !m.StudentRead)
                    .ToList()
                    .ForEach(m => m.StudentRead = true);
            }

            await _chatRepository.UpdateAsync(chat);

            return NoContent();
        }
        [HttpPost("create")]
        public async Task<ActionResult<Chat>> CreateWithEmail([FromQuery] string email)
        {
            // Find the user to chat with by email
            var otherUser = await _userService.GetByEmailAsync(email);
            if (otherUser == null)
            {
                return NotFound($"User with email '{email}' not found.");
            }

            // Get the current user
            var currentUser = await _userService.GetOrCreateUserAsync(User);

            // Prevent creating a chat with yourself
            if (currentUser.Id == otherUser.Id)
            {
                return BadRequest("Cannot create a chat with yourself.");
            }

            // Check if a chat already exists between these users
            var existingChat = await _chatRepository.Query()
                .FirstOrDefaultAsync(c =>
                    (c.SlbApplicationUserId == currentUser.Id && c.StudentApplicationUserId == otherUser.Id) ||
                    (c.SlbApplicationUserId == otherUser.Id && c.StudentApplicationUserId == currentUser.Id));

            if (existingChat != null)
            {
                return Ok(existingChat);
            }
            //todo dit aanpassen
            // Decide who is SLB and who is Student (adjust logic as needed)
            var chat = new Chat
            {
                SlbApplicationUserId = currentUser.Id,
                StudentApplicationUserId = otherUser.Id
            };

            await _chatRepository.AddAsync(chat);

            return CreatedAtAction(nameof(Read), new { id = chat.Id }, chat);
        }


    }
}
