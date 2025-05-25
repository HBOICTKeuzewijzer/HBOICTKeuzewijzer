using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HBOICTKeuzewijzer.Api.Models
{
    public class Module : IEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(20)]
        [Required]
        public string Code { get; set; } = string.Empty;

        [Column(TypeName = "text")]
        public string? Description { get; set; }

        [Column(TypeName = "text")]
        public string? PrerequisiteJson { get; set; }

        [Required]
        public int ECs { get; set; }

        [Required]
        public int Level { get; set; }

        [Required]
        public bool Required { get; set; } = false;

        public int? RequiredSemester { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public Category? Category { get; set; }

        public Guid? CategoryId { get; set; }

        [ForeignKey(nameof(OerId))]
        public Oer? Oer { get; set; }

        [Required]
        public Guid OerId { get; set; }

        public ICollection<Semester>? Semesters { get; set; }
    }
}
