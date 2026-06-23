namespace TemplateJwtProject.Models.DTOs;

public class RouteResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int EstimatedTimeMinutes { get; set; }
    public List<RouteLocationResponseDto> Locations { get; set; } = new();
}
