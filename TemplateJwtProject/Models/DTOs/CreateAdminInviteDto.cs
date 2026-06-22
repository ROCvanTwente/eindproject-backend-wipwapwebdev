using System.ComponentModel.DataAnnotations;

namespace TemplateJwtProject.Models.DTOs;

public class CreateAdminInviteDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
