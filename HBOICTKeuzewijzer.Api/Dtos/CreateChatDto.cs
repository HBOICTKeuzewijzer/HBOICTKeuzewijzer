using System.ComponentModel.DataAnnotations;

namespace HBOICTKeuzewijzer.Api.Dtos
{
    public class CreateChatDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

    }
}
