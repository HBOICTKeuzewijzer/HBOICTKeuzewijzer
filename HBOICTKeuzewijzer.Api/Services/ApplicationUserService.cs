using HBOICTKeuzewijzer.Api.DAL;
using HBOICTKeuzewijzer.Api.DAL.Entities;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace HBOICTKeuzewijzer.Api.Services
{
    public class ApplicationUserService(AppDbContext appDbContext)
    {
        public async Task<ApplicationUser> GetOrCreateUserAsync(ClaimsPrincipal principal)
        {
            var externalId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = principal.FindFirst(ClaimTypes.Email)?.Value ?? "(unknown)";
            var displayName = principal.FindFirst("http://schemas.microsoft.com/identity/claims/displayname")?.Value
                              ?? principal.Identity?.Name
                              ?? "Unknown";

            if (string.IsNullOrWhiteSpace(externalId))
                throw new InvalidOperationException("Missing external user ID.");

            var user = await appDbContext.ApplicationUsers
                .FirstOrDefaultAsync(u => u.ExternalId == externalId);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    ExternalId = externalId,
                    Email = email,
                    DisplayName = displayName
                };

                appDbContext.ApplicationUsers.Add(user);
                await appDbContext.SaveChangesAsync();
            }

            return user;
        }
    }
}
