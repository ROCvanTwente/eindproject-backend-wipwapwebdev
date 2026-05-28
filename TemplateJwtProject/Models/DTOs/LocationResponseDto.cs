namespace TemplateJwtProject.Models.DTOs;

public class LocationResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Floor { get; set; }
    public double XCoordinate { get; set; }
    public double YCoordinate { get; set; }
    public int BuildingId { get; set; }
    public string BuildingName { get; set; } = string.Empty;
}
