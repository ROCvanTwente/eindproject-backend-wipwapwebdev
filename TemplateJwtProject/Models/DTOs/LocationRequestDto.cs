using System.ComponentModel.DataAnnotations;

namespace TemplateJwtProject.Models.DTOs;

public class LocationRequestDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(5000)]
    public string Description { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }

    [Range(-5, 50)]
    public int Floor { get; set; }

    public double XCoordinate { get; set; }
    public double YCoordinate { get; set; }

    [Required]
    public int BuildingId { get; set; }
}
