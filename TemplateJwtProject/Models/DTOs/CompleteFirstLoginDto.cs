using System.ComponentModel.DataAnnotations;

namespace TemplateJwtProject.Models.DTOs;

public class CompleteFirstLoginDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(12, MinimumLength = 6)]
    public string ActivationCode { get; set; } = string.Empty;

    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string NewPassword { get; set; } = string.Empty;
}
