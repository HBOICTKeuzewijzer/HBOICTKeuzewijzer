using HBOICTKeuzewijzer.Api.Models;
using HBOICTKeuzewijzer.Api.Repositories;
using HBOICTKeuzewijzer.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace HBOICTKeuzewijzer.Api.Controllers
{
    [Route("Chat/[controller]")]
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
        public async Task<ActionResult<PaginatedResult<Message>>> List([FromQuery] GetAllRequestQuery request)
        {
            return Ok(await _messageRepository.GetPaginatedAsync(request));
        }

        [HttpPost]
        public async Task<ActionResult<PaginatedResult<Message>>> Create([FromQuery] GetAllRequestQuery request)
        {
            return Ok(await _messageRepository.GetPaginatedAsync(request));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<PaginatedResult<Message>>> Update([FromQuery] GetAllRequestQuery request)
        {
            return Ok(await _messageRepository.GetPaginatedAsync(request));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<PaginatedResult<Message>>> Delete([FromQuery] GetAllRequestQuery request)
        {
            return Ok(await _messageRepository.GetPaginatedAsync(request));
        }
    }
}
