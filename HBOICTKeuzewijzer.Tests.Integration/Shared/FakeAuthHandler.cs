using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HBOICTKeuzewijzer.Tests.Integration.Shared
{
    public class FakeAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string AuthenticationScheme = "Test";

        /// <summary>
        /// A way to test our endpoints with specific user roles. When running a test that requires a specific role add the following to the request headers:
        /// `request.Headers.Add("X-Test-Role", "The role");`
        /// </summary>
        /// <returns></returns>
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var role = Request.Headers["X-Test-Role"].FirstOrDefault();
            var claims = new List<Claim>();

            if (!string.IsNullOrWhiteSpace(role))
            {
                claims.Add(new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", role));
            }

            claims.AddRange(new[]
            {
                new Claim(ClaimTypes.Name, "Test User"),
                new Claim(ClaimTypes.NameIdentifier, "6457A1CF-FAE1-46B5-BF24-C9D655BBA4EF"),
                new Claim(ClaimTypes.Email, "test@example.com"),
                new Claim("http://schemas.microsoft.com/identity/claims/displayname", "Test User")
            });

            var identity = new ClaimsIdentity(claims, AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, AuthenticationScheme);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        public FakeAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }
    }
}