using System;
using System.ComponentModel.DataAnnotations;

namespace HBOICTKeuzewijzer.Api.Models
{
    public class ModuleReview : IEntity
    {
        public int Id { get; set; }

        [Required]
        public Guid StudentId { get; set; }
        public ApplicationUser Student { get; set; }

        [Required]
        public Guid ModuleId { get; set; }
        public Module? Module { get; set; }

        [Required]
        [MaxLength(1000)]
        public string? ReviewText { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
