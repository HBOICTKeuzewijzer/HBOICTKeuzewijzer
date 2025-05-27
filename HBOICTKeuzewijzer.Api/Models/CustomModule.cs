using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HBOICTKeuzewijzer.Api.Models
{
    public class CustomModule : IEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [Column(TypeName = "text")]
        public string? Description { get; set; }

        [Required] 
        public int ECs { get; set; } = 30;

        public Semester? Semester { get; set; }
    }
}
