using System.ComponentModel.DataAnnotations;

namespace TemplateJwtProject.Models;

public class Route
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(20000)]
    public string Description { get; set; } = string.Empty;

    [Range(1, 600)]
    public int EstimatedTimeMinutes { get; set; }

    public ICollection<RouteLocation> RouteLocations { get; set; } = new List<RouteLocation>();
}
