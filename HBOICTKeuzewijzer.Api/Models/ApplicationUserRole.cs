using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HBOICTKeuzewijzer.Api.Models
{
    public class ApplicationUserRole : IEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Role Role { get; set; }

        [Required]
        public Guid ApplicationUserId { get; set; }

        [ForeignKey(nameof(ApplicationUserId))]
        public ApplicationUser? ApplicationUser { get; set; }
    }
}
