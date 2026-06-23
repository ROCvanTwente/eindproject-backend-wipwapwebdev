using System.ComponentModel.DataAnnotations;

namespace TemplateJwtProject.Models.DTOs;

public class LoginDto
{
    public string Identifier { get; set; } = string.Empty;

    [EmailAddress]
    public string? Email { get; set; }

    [Required]
    public string Password { get; set; } = string.Empty;
}
