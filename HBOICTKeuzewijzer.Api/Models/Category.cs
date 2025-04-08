using System.ComponentModel.DataAnnotations;

namespace HBOICTKeuzewijzer.Api.Models
{
    public class Category
    {
        [Key]
        public Guid Id { get; set; }

        [MaxLength(255)]
        public string? Value { get; set; }
        public ICollection<Module>? Modules { get; set; }

    }
}
