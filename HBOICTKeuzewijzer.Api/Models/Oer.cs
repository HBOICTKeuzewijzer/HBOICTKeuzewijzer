using System.ComponentModel.DataAnnotations;

namespace HBOICTKeuzewijzer.Api.Models
{
    public class Oer : IEntity
    {
        [Key]
        public Guid Id { get; set; }

        [MaxLength(260)]
        public string? Filepath { get; set; }

        [Required]
        [MaxLength(5)]
        public string AcademicYear { get; set; } = string.Empty;

        public ICollection<Module>? Modules { get; set; }
    }
}
