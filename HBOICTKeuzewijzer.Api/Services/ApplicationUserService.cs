using HBOICTKeuzewijzer.Api.DAL;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using HBOICTKeuzewijzer.Api.Models;

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
                var roleClaim = principal.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;

                var roleEnum = roleClaim switch
                {
                    "Student" => Role.Student,
                    "SLB" => Role.SLB,
                    "ModuleAdmin" => Role.ModuleAdmin,
                    "SystemAdmin" => Role.SystemAdmin,
                    _ => Role.User
                };

                user = new ApplicationUser
                {
                    ExternalId = externalId,
                    Email = email,
                    DisplayName = displayName
                    //Role = roleEnum
                };

                appDbContext.ApplicationUsers.Add(user);
                await appDbContext.SaveChangesAsync();
            }

            return user;
        }
    }
}
