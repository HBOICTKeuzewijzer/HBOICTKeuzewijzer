using HBOICTKeuzewijzer.Api.Dtos;
using HBOICTKeuzewijzer.Api.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HBOICTKeuzewijzer.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IApplicationUserService _applicationUserService;

        public AuthController(IApplicationUserService applicationUserService)
        {
            _applicationUserService = applicationUserService;
        }

        [HttpGet("login")]
        public IActionResult Login([FromQuery] string returnUrl = "")
        {
            return Challenge(new AuthenticationProperties
            {
                RedirectUri = $"/auth/succes?returnUrl={Uri.EscapeDataString(returnUrl)}"
            }, "Saml2");
        }

        [HttpGet("succes")]
        [Authorize]
        public async Task<IActionResult> Succes([FromQuery] string returnUrl = "/")
        {
            var user = await _applicationUserService.GetOrCreateUserAsync(User);

            return Redirect(returnUrl);
        }

        [HttpGet("logout")]
        public IActionResult Logout([FromQuery] string returnUrl = "/")
        {
            return SignOut(new AuthenticationProperties
            {
                RedirectUri = $"/auth/logout-succes?returnUrl={Uri.EscapeDataString(returnUrl)}"
            });
        }

        [HttpGet("logout-succes")]
        public IActionResult LogoutSucces([FromQuery] string returnUrl = "/")
        {
            return Redirect(returnUrl);
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> Me()
        {
            var user = await _applicationUserService.GetOrCreateUserAsync(User);

            var authResult = await HttpContext.AuthenticateAsync();
            var expiresUtc = authResult.Properties?.ExpiresUtc;

            var dto = new ApplicationUserDto
            {
                Id = user.Id,
                DisplayName = user.DisplayName,
                Email = user.Email,
                Code = user.Code,
                Cohort = user.Cohort,
                SessionExpiresAt = expiresUtc?.UtcDateTime,
                ApplicationUserRoles = user.ApplicationUserRoles
            };

            return Ok(dto);
        }
    }
}
