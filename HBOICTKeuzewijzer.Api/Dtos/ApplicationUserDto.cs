using HBOICTKeuzewijzer.Api.Models;

namespace HBOICTKeuzewijzer.Api.Dtos;

public class ApplicationUserDto
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; }
    public string? Email { get; set; }
    public string Code { get; set; }
    public string? Cohort { get; set; }
    public DateTime? SessionExpiresAt { get; set; }
    public ICollection<ApplicationUserRole>? ApplicationUserRoles { get; set; }
}