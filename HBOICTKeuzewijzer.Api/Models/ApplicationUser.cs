﻿using System.ComponentModel.DataAnnotations;

namespace HBOICTKeuzewijzer.Api.Models
{
    public class ApplicationUser
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string ExternalId { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public Role Role { get; set; }

        [Required]
        [StringLength(100)]
        public string DisplayName { get; set; } = string.Empty;
    }
}
