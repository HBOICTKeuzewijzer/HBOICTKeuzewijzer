using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace HBOICTKeuzewijzer.Api.Models
{
    public class Module
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(20)]
        [Required]
        public string Code { get; set; } = string.Empty;

        [MaxLength(200)]
        [Column(TypeName = "text")]
        public string? Description { get; set; }

        [MaxLength(200)]
        [Required]
        public string? PrerequisiteJson { get; set; }

        [Required]
        public int ECs { get; set; }
        [Required]
        public int Level { get; set; }

        [ForeignKey(nameof(CategoryId))]
        [JsonIgnore]
        public Category? Category { get; set; }
        public Guid? CategoryId { get; set; }

        [ForeignKey(nameof(OerId))]
        public Oer? Oer { get; set; }

        [Required]
        public Guid OerId { get; set; }

        public ICollection<Semester>? Semesters { get; set; }
    }
}
