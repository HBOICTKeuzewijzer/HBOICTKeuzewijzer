using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HBOICTKeuzewijzer.Api.Models
{
    public class Chat
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [ForeignKey("SLB")]
        public Guid SlbApplicationUserId { get; set; }

        [ForeignKey(nameof(SlbApplicationUserId))]
        public ApplicationUser? SLB { get; set; }

        [Required]
        [ForeignKey("Student")]
        public Guid StudentApplicationUserId { get; set; }

        [ForeignKey(nameof(StudentApplicationUserId))]
        public ApplicationUser? Student { get; set; }
        public ICollection<Message>? Messages { get; set; }


    }
}
