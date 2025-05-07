using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HBOICTKeuzewijzer.Api.Models
{
    public class Semester
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public int Index { get; set; }

        [Required]
        public int AcquiredECs { get; set; }

        public Guid? ModuleId { get; set; }

        [ForeignKey(nameof(ModuleId))]
        public Module? Module { get; set; }

        [Required]
        public Guid StudyRouteId { get; set; }

        [ForeignKey(nameof(StudyRouteId))]
        public StudyRoute? StudyRoute { get; set; }
    }

}