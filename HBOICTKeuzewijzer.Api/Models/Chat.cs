using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HBOICTKeuzewijzer.Api.Models
{
    public class Chat : IEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [ForeignKey("SLB")]
        public Guid SlbApplicationUserId { get; set; }

        [ForeignKey(nameof(SlbApplicationUserId))]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public ApplicationUser? SLB { get; set; }

        [Required]
        [ForeignKey("Student")]
        public Guid StudentApplicationUserId { get; set; }

        [ForeignKey(nameof(StudentApplicationUserId))]
        [DeleteBehavior(DeleteBehavior.Cascade)]
        public ApplicationUser? Student { get; set; }
        
        public ICollection<Message>? Messages { get; set; }
    }
}
