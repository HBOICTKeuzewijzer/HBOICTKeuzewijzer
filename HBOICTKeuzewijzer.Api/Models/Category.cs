using System.ComponentModel.DataAnnotations;

namespace HBOICTKeuzewijzer.Api.Models
{
    public class Category : IEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Value { get; set; } = string.Empty;

        [Required]
        [MaxLength(7)]
        public string PrimaryColor { get; set; } = "#ffffff";

        [Required]
        [MaxLength(7)]
        public string AccentColor { get; set; } = "#ffffff";

        public int? Position { get; set; }

        public ICollection<Module>? Modules { get; set; }

    }
}
