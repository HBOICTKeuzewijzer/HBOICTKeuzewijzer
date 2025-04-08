using System.ComponentModel.DataAnnotations;

namespace HBOICTKeuzewijzer.Api.Models
{
    public class ApplicationUser
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(255)]
        public string ExternalId { get; set; } 

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string? Email { get; set; } 


        [Required]
        [StringLength(100)]
        public string DisplayName { get; set; } 

        [Required]
        [StringLength(10)]
        public string Code { get; set; } 


        public ICollection<StudyRoute>? StudyRoutes { get; set; }

        public ICollection<ApplicationUserRole>? ApplicationUserRoles { get; set; }
    }
}
