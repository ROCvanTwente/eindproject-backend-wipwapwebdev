using System.ComponentModel.DataAnnotations;

namespace TemplateJwtProject.Models.DTOs;

public class BuildingRequestDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(20000)]
    public string Description { get; set; } = string.Empty;
}
