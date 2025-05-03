using HBOICTKeuzewijzer.Api.Models;
using HBOICTKeuzewijzer.Api.Repositories;
using HBOICTKeuzewijzer.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace HBOICTKeuzewijzer.Api.Controllers
{
    /*
    * The Message logic is separated from the Chat logic to keep the ChatController concise
    * and maintain a clear separation of concerns.
    *
    * Although it's a separate controller, MessageController is still routed under "Chat" 
    * because messages are a child resource of chats.
    */

    [Route("Chat/{chatId}/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly IRepository<Message> _messageRepository;
        private readonly IRepository<Chat> _chatRepository;
        private readonly ApplicationUserService _userService;

        public MessageController(
            IRepository<Message> messageRepository,
            IRepository<Chat> chatRepository,
            ApplicationUserService userService)
        {
            _messageRepository = messageRepository;
            _chatRepository = chatRepository;
            _userService = userService;
        }

        private async Task<Chat?> GetAuthorizedChat(Guid chatId)
        {
            var user = await _userService.GetOrCreateUserAsync(User);
            var chat = await _chatRepository.GetByIdAsync(chatId);

            if (chat == null ||
                (chat.SlbApplicationUserId != user.Id && chat.StudentApplicationUserId != user.Id))
            {
                return null;
            }

            return chat;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResult<Message>>> List(Guid chatId, [FromQuery] GetAllRequestQuery request)
        {
            // Ensure the user is allowed to view the messages
            var chat = await GetAuthorizedChat(chatId);
            if (chat == null) return NotFound();

            // Get all the messages associated with the chat.
            var result = await _messageRepository
                .GetPaginatedAsync(request, m => (m as Message)!.ChatId == chatId);

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Message>> Read(Guid chatId, Guid id)
        {
            // Ensure the user is allowed to view the messages
            var chat = await GetAuthorizedChat(chatId);
            if (chat == null) return NotFound();

            var message = await _messageRepository.GetByIdAsync(id);

            // Check if the message exists and is associated with the chat.
            if (message == null || message.ChatId != chatId)
                return NotFound();

            return Ok(message);
        }

        [HttpPost]
        public async Task<ActionResult<Message>> Create(Guid chatId, [FromBody] Message message)
        {
            // Ensure the user is allowed to view the messages
            var chat = await GetAuthorizedChat(chatId);
            if (chat == null) return NotFound();

            // Set the current chatID
            message.ChatId = chatId;
            await _messageRepository.AddAsync(message);

            return CreatedAtAction(nameof(Create), new { chatId, id = message.Id }, message);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(Guid chatId, Guid id, [FromBody] Message updatedMessage)
        {
            // Ensure the user is allowed to view the messages
            var chat = await GetAuthorizedChat(chatId);
            if (chat == null) return NotFound();

            // Check if the message exists and is associated with the chat.
            var existing = await _messageRepository.GetByIdAsync(id);
            if (existing == null || existing.ChatId != chatId)
                return NotFound();

            // Set the current ID & ChatId since we dont want to overwrite these
            updatedMessage.Id = id;
            updatedMessage.ChatId = chatId;

            await _messageRepository.UpdateAsync(updatedMessage);
        
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid chatId, Guid id)
        {
            // Ensure the user is allowed to view the messages
            var chat = await GetAuthorizedChat(chatId);
            if (chat == null) return NotFound();

            // Check if the message exists and is associated with the chat.
            var existing = await _messageRepository.GetByIdAsync(id);
            if (existing == null || existing.ChatId != chatId)
                return NotFound();

            await _messageRepository.DeleteAsync(id);
            
            return NoContent();
        }
    }
}
