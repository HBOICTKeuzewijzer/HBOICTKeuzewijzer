using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HBOICTKeuzewijzer.Api.Models
{
    public class Message
    {
        [Key]
        public Guid Id { get; set; }

        [MaxLength(500)]
        public string? MessageText { get; set; }

        [Required]
        public DateTime SentAt { get; set; }

        [ForeignKey(nameof(ChatId))]
        public Chat? Chat { get; set; }

        [Required]
        public Guid ChatId { get; set; }

    }
}
