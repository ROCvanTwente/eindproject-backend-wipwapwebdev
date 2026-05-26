using System.ComponentModel.DataAnnotations;

namespace TemplateJwtProject.Models;

public class Location
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Range(-5, 50)]
    public int Floor { get; set; }

    public double XCoordinate { get; set; }

    public double YCoordinate { get; set; }

    [Required]
    public int BuildingId { get; set; }

    public Building? Building { get; set; }

    public ICollection<RouteLocation> RouteLocations { get; set; } = new List<RouteLocation>();
}
