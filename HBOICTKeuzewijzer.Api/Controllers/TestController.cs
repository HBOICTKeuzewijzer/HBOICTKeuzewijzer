using HBOICTKeuzewijzer.Api.Attributes;
using HBOICTKeuzewijzer.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HBOICTKeuzewijzer.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet("default")]
        [Authorize]
        public IActionResult Default()
        {
            return Ok("You're authenticated!");
        }

        [HttpGet("student")]
        [EnumAuthorize(Role.Student)]
        public IActionResult Student()
        {
            return Ok("You're a student!");
        }

        [HttpGet("admin")]
        [EnumAuthorize(Role.SystemAdmin)]
        public IActionResult Admin()
        {
            return Ok("You're an admin!");
        }

        [HttpGet("enumroles")]
        [EnumAuthorize(Role.SLB, Role.Student, Role.ModuleAdmin)]
        public IActionResult TestMultipleRoles()
        {
            return Ok("You're an admin!");
        }
    }
}
