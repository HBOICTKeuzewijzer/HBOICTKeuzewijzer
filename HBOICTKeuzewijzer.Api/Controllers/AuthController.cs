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
                RedirectUri = $"/auth/success?returnUrl={Uri.EscapeDataString(returnUrl)}"
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

        [HttpGet("role-by-id")]
        [Authorize]
        public async Task<IActionResult> GetRoleById([FromQuery] Guid id)
        {
            var user = await _applicationUserService.GetUserWithRolesByIdAsync(id);

            if (user == null)
                return NotFound("User not found.");

            var roles = user.ApplicationUserRoles?.Select(r => r.Role).ToList();
            if (roles == null || !roles.Any())
                return Ok(new { Id = id, Role = "None" });

            return Ok(new { Id = id, Roles = roles.Select(r => r.ToString()).ToList() });
        }


    }
}
