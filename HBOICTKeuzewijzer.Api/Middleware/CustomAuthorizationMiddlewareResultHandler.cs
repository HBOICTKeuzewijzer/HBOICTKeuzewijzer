using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Authorization;

namespace HBOICTKeuzewijzer.Api.Middleware
{
    public class CustomAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
    {
        private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

        public async Task HandleAsync(
            RequestDelegate next,
            HttpContext context,
            AuthorizationPolicy policy,
            PolicyAuthorizationResult authorizeResult)
        {
            if (!authorizeResult.Succeeded)
            {
                    context.Response.StatusCode = authorizeResult.Forbidden
                        ? StatusCodes.Status403Forbidden
                        : StatusCodes.Status401Unauthorized;

                    await context.Response.WriteAsync(authorizeResult.Forbidden ? "Forbidden" : "Unauthorized");
                    return;
            }

            await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
        }
    }
}
