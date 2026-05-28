using System.ComponentModel.DataAnnotations;

namespace TemplateJwtProject.Models.DTOs;

public class ForcePasswordChangeDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string NewPassword { get; set; } = string.Empty;
}
