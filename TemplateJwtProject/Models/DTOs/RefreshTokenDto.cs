using System.ComponentModel.DataAnnotations;

namespace TemplateJwtProject.Models.DTOs;

public class RefreshTokenDto
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
