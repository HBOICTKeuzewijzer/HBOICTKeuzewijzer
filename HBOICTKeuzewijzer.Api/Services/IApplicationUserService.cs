using System.Security.Claims;
using HBOICTKeuzewijzer.Api.DAL;
using HBOICTKeuzewijzer.Api.Models;

namespace HBOICTKeuzewijzer.Api.Services
{
    public interface IApplicationUserService
    {
        Task<ApplicationUser> GetOrCreateUserAsync(ClaimsPrincipal principal);
        Task<ApplicationUser?> GetByPrincipal(ClaimsPrincipal principal);
        Task<ApplicationUser?> GetByEmailAsync(string email);
        Task<ApplicationUser?> GetUserWithRolesByIdAsync(Guid id);
    }
}
