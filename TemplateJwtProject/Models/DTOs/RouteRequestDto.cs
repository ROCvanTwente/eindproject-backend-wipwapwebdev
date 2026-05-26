using System.ComponentModel.DataAnnotations;

namespace TemplateJwtProject.Models.DTOs;

public class RouteRequestDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Range(1, 600)]
    public int EstimatedTimeMinutes { get; set; }

    public List<RouteLocationRequestDto> Locations { get; set; } = new();
}
