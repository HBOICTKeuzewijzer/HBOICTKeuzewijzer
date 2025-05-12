using System.ComponentModel.DataAnnotations;

namespace HBOICTKeuzewijzer.Api.Models
{
    public class ApplicationUser
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(255)]
        public string ExternalId { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string DisplayName { get; set; } = string.Empty;

        [StringLength(10)]
        public string? Code { get; set; }

        [MaxLength(5)]
        public string? Cohort { get; set; }

        public ICollection<StudyRoute>? StudyRoutes { get; set; }

        public ICollection<ApplicationUserRole>? ApplicationUserRoles { get; set; }
    }
}
