using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HBOICTKeuzewijzer.Api.Models
{
    public class Slb
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid SlbApplicationUserId { get; set; }
        public ApplicationUser? SlbApplicationUser { get; set; }

        [Required]
        public Guid StudentApplicationUserId { get; set; }
        public ApplicationUser? StudentApplicationUser { get; set; }
    }
}
