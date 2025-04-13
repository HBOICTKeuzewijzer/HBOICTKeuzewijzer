using HBOICTKeuzewijzer.Api.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HBOICTKeuzewijzer.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationUserService _applicationUserService;

        public AuthController(ApplicationUserService applicationUserService)
        {
            _applicationUserService = applicationUserService;
        }

        [HttpGet("login")]
        public IActionResult Login([FromQuery] string returnUrl = "")
        {
            return Challenge(new AuthenticationProperties
            {
                RedirectUri = $"/api/auth/success?returnUrl={Uri.EscapeDataString(returnUrl)}"
            }, "Saml2");
        }

        [HttpGet("success")]
        [Authorize]
        public async Task<IActionResult> Success([FromQuery] string returnUrl = "/")
        {
            var user = await _applicationUserService.GetOrCreateUserAsync(User);

            return Redirect(returnUrl);
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> Me()
        {
            var user = await _applicationUserService.GetOrCreateUserAsync(User);

            return Ok(user);
        }
    }
}
