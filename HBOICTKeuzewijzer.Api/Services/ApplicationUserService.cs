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
            var externalId = GetExternalId(principal);
            var email = GetEmail(principal);
            var displayName = GetDisplayName(principal);
            var roleClaims = GetRolesFromClaims(principal);

            var user = await appDbContext.ApplicationUsers
                .Include(u => u.ApplicationUserRoles)
                .FirstOrDefaultAsync(u => u.ExternalId == externalId);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    ExternalId = externalId,
                    Email = email,
                    DisplayName = displayName,
                    Code = "default"
                };

                appDbContext.ApplicationUsers.Add(user);
            }
            else
            {
                // Update user's display name or email if changed externally
                user.Email = email;
                user.DisplayName = displayName;
            }

            SyncUserRoles(user, roleClaims);

            await appDbContext.SaveChangesAsync();

            return user;
        }

        public async Task<ApplicationUser?> GetByPrincipal(ClaimsPrincipal principal)
        {
            var externalId = GetExternalId(principal);

            return await appDbContext.ApplicationUsers
                .Include(u => u.ApplicationUserRoles)
                .FirstOrDefaultAsync(u => u.ExternalId == externalId);
        }

        private static string GetExternalId(ClaimsPrincipal principal)
        {
            var id = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(id))
                throw new InvalidOperationException("Missing external user ID.");
            return id;
        }

        private static string GetEmail(ClaimsPrincipal principal) =>
            principal.FindFirst(ClaimTypes.Email)?.Value ?? "(unknown)";

        private static string GetDisplayName(ClaimsPrincipal principal) =>
            principal.FindFirst("http://schemas.microsoft.com/identity/claims/displayname")?.Value
            ?? principal.Identity?.Name
            ?? "Unknown";

        private static List<Role> GetRolesFromClaims(ClaimsPrincipal principal) =>
            principal
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

        private void SyncUserRoles(ApplicationUser user, List<Role> currentRolesFromClaims)
        {
            // Ensure ApplicationUserRoles is not null
            user.ApplicationUserRoles ??= new List<ApplicationUserRole>();

            var existingRoles = user.ApplicationUserRoles.Select(ur => ur.Role).ToList();

            // Add new roles not yet assigned
            var rolesToAdd = currentRolesFromClaims.Except(existingRoles).ToList();
            foreach (var role in rolesToAdd)
            {
                appDbContext.ApplicationUserRoles.Add(new ApplicationUserRole
                {
                    Role = role,
                    ApplicationUsers = user
                });
            }

            // Remove roles that are no longer present in claims
            var rolesToRemove = user.ApplicationUserRoles
                .Where(ur => !currentRolesFromClaims.Contains(ur.Role))
                .ToList();

            if (rolesToRemove.Count > 0)
            {
                appDbContext.ApplicationUserRoles.RemoveRange(rolesToRemove);
            }
        }
    }
}
