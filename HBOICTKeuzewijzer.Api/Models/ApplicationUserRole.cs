using System.ComponentModel.DataAnnotations;

namespace HBOICTKeuzewijzer.Api.Models
{
    public class ApplicationUserRole
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Role Role { get; set; }

        [Required]
        public Guid ApplicationUserId { get; set; } 
        public ApplicationUser? ApplicationUsers { get; set; } 

    }
}
