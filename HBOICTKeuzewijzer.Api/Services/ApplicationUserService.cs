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
                .Include(u => u.ApplicationUserRoles)
                .FirstOrDefaultAsync(u => u.ExternalId == externalId);

            if (user == null)
            {
                var roleClaims = principal
                    .FindAll("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
                    .Select(c => c.Value)
                    .Select(role => role switch
                    {
                        "Student" => Role.Student,
                        "SLB" => Role.SLB,
                        "ModuleAdmin" => Role.ModuleAdmin,
                        "SystemAdmin" => Role.SystemAdmin,
                        _ => Role.User
                    })
                    .Distinct()
                    .ToList();

                user = new ApplicationUser
                {
                    ExternalId = externalId,
                    Email = email,
                    DisplayName = displayName,
                    Code = "default"
                };

                appDbContext.ApplicationUsers.Add(user);

                foreach (var roleClaim in roleClaims)
                {
                    ApplicationUserRole userRole = new()
                    {
                        Role = roleClaim,
                        ApplicationUsers = user
                    };

                    appDbContext.ApplicationUserRoles.Add(userRole);
                }
               
                await appDbContext.SaveChangesAsync();
            }

            return user;
        }

        public async Task<ApplicationUser?> GetByPrincipal(ClaimsPrincipal principal)
        {
            var externalId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(externalId))
                throw new InvalidOperationException("Missing external user ID.");

            return await appDbContext.ApplicationUsers
                .Include(u => u.ApplicationUserRoles)
                .FirstOrDefaultAsync(u => u.ExternalId == externalId);
        }
        public async Task<ApplicationUser?> GetByEmailAsync(string email)
        {
            return await appDbContext.ApplicationUsers
                .FirstOrDefaultAsync(u => u.Email == email);
        }
        public async Task<ApplicationUser?> GetUserWithRolesByIdAsync(Guid id)
        {
            return await appDbContext.ApplicationUsers
                .Include(u => u.ApplicationUserRoles)
                .FirstOrDefaultAsync(u => u.Id == id);
        }




    }
}
