using System.ComponentModel.DataAnnotations;

namespace TemplateJwtProject.Models.DTOs;

public class RouteLocationRequestDto
{
    [Required]
    public int LocationId { get; set; }

    [Range(0, 500)]
    public int Order { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}
