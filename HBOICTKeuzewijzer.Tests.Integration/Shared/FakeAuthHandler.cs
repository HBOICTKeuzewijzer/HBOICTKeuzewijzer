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
        /// A fake authentication handler used for integration testing.
        /// By default, requests are treated as unauthenticated and will result in a 401 Unauthorized response.
        ///
        /// To simulate an authenticated user, include the following header:
        /// <code>
        /// X-Test-Auth: true
        /// </code>
        ///
        /// Optional headers to customize the authenticated user:
        /// <list type="bullet">
        ///   <item>
        ///     <term>X-Test-Role</term>
        ///     <description>The user's role (e.g., "SystemAdmin", "ModuleAdmin").</description>
        ///   </item>
        ///   <item>
        ///     <term>X-User-Id</term>
        ///     <description>The user's ID (defaults to a fixed test GUID if omitted).</description>
        ///   </item>
        ///   <item>
        ///     <term>X-User-Name</term>
        ///     <description>The user's display name (defaults to "Test User").</description>
        ///   </item>
        ///   <item>
        ///     <term>X-User-Email</term>
        ///     <description>The user's email address (defaults to "test@example.com").</description>
        ///   </item>
        /// </list>
        /// </summary>
        /// <returns>
        /// An <see cref="AuthenticateResult"/> indicating whether the request is authenticated or not.
        /// </returns>

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Default: not authenticated unless explicitly opted in
            if (!Request.Headers.TryGetValue("X-Test-Auth", out var authHeader) ||
                !authHeader.ToString().Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(AuthenticateResult.NoResult()); // triggers 401
            }

            // Proceed with fake authenticated user
            var role = Request.Headers["X-Test-Role"].FirstOrDefault();
            var userId = Request.Headers["X-User-Id"].FirstOrDefault() ?? "6457A1CF-FAE1-46B5-BF24-C9D655BBA4EF";
            var name = Request.Headers["X-User-Name"].FirstOrDefault() ?? "Test User";
            var email = Request.Headers["X-User-Email"].FirstOrDefault() ?? "test@example.com";

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, name),
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, email),
                new Claim("http://schemas.microsoft.com/identity/claims/displayname", name)
            };

            if (!string.IsNullOrWhiteSpace(role))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

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