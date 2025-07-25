using System.ComponentModel.DataAnnotations;

namespace HBOICTKeuzewijzer.Api.Dtos
{
    public class ModuleReviewDto
    {
        public Guid ModuleId { get; set; }

        [Required]
        [MinLength(5)]
        public string ReviewText { get; set; }
    }

    public class ModuleReviewResponseDto
    {
        public string StudentName { get; set; }
        public string ReviewText { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
