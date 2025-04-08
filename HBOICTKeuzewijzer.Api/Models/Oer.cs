using System.ComponentModel.DataAnnotations;

namespace HBOICTKeuzewijzer.Api.Models
{
        public class Oer
        {
            [Key]
            public Guid Id { get; set; }

            [MaxLength(260)]
            public string? Filepath { get; set; }  

            [Required]
            [MaxLength(5)]
            public string? AcademicYear { get; set; }  
            public ICollection<Module>? Modules { get; set; }
        }
    }
