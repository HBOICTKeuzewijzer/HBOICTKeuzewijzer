using HBOICTKeuzewijzer.Api.Models;

namespace HBOICTKeuzewijzer.Api.Dtos
{
    public class StudentDto
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; }
        public string? Email { get; set; }
        public string Code { get; set; }
        public string? Cohort { get; set; }

        public Guid? SlbId { get; set; }
    }
}
