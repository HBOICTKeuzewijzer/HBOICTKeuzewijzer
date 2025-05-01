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
            return Ok(await _chatRepository.GetPaginatedAsync(request));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PaginatedResult<Chat>>> Read([FromQuery] GetAllRequestQuery request)
        {
            return Ok(await _chatRepository.GetPaginatedAsync(request));
        }

        [HttpPost]
        public async Task<ActionResult<PaginatedResult<Chat>>> Create([FromQuery] GetAllRequestQuery request)
        {
            return Ok(await _chatRepository.GetPaginatedAsync(request));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<PaginatedResult<Chat>>> Update([FromQuery] GetAllRequestQuery request)
        {
            return Ok(await _chatRepository.GetPaginatedAsync(request));
        }
    }
}
