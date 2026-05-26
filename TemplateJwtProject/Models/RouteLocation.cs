using System.ComponentModel.DataAnnotations;

namespace TemplateJwtProject.Models;

public class RouteLocation
{
    [Required]
    public int RouteId { get; set; }

    public Route? Route { get; set; }

    [Required]
    public int LocationId { get; set; }

    public Location? Location { get; set; }

    [Range(1, 500)]
    public int Order { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}
