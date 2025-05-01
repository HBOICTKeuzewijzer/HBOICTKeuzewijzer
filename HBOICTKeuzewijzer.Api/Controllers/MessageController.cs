using HBOICTKeuzewijzer.Api.Models;
using HBOICTKeuzewijzer.Api.Repositories;
using HBOICTKeuzewijzer.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace HBOICTKeuzewijzer.Api.Controllers
{
    /*
    * I have chosen to seperate the Message logic from the chat logic.
    * This is done to keep the chat controller small an seperate the logic.
    *
    * We kept the Message controller bundeld under the chat route as it is part of that entity
    */

    [Route("Chat/{chatId}/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly IRepository<Message> _messageRepository;
        private readonly ApplicationUserService _userService;

        public MessageController(IRepository<Message> messageRepository, ApplicationUserService userService)
        {
            _messageRepository = messageRepository;
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResult<Message>>> List(Guid chatId, [FromQuery] GetAllRequestQuery request)
        {
            // Get all the messages associated with the chat.
            var result = await _messageRepository
                .GetPaginatedAsync(request, m => (m as Message)!.ChatId == chatId);

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Message>> GetById(Guid chatId, Guid id)
        {
            var message = await _messageRepository.GetByIdAsync(id);
            if (message == null || message.ChatId != chatId)
                return NotFound();

            return Ok(message);
        }

        [HttpPost]
        public async Task<ActionResult<Message>> Create(Guid chatId, [FromBody] Message message)
        {
            message.ChatId = chatId;
            await _messageRepository.AddAsync(message);
            return CreatedAtAction(nameof(GetById), new { chatId = chatId, id = message.Id }, message);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(Guid chatId, Guid id, [FromBody] Message updatedMessage)
        {
            var existing = await _messageRepository.GetByIdAsync(id);
            if (existing == null || existing.ChatId != chatId)
                return NotFound();

            updatedMessage.Id = id;
            updatedMessage.ChatId = chatId;
            await _messageRepository.UpdateAsync(updatedMessage);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid chatId, Guid id)
        {
            var existing = await _messageRepository.GetByIdAsync(id);
            if (existing == null || existing.ChatId != chatId)
                return NotFound();

            await _messageRepository.DeleteAsync(id);
            return NoContent();
        }
    }
}
