using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace HBOICTKeuzewijzer.Api.Models
{
    public class ApplicationUserRole
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Role Role { get; set; }

        [Required]
        public Guid ApplicationUserId { get; set; } // foreign key



        public ApplicationUser applicationUser { get; set; } // navigation property
        


    }
}
