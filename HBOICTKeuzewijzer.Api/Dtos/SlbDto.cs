using HBOICTKeuzewijzer.Api.Models;

namespace HBOICTKeuzewijzer.Api.Dtos
{
    public class SlbDto
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; }
        public string? Email { get; set; }

        public ICollection<StudentDto>? Students { get; set; }
    }
}
