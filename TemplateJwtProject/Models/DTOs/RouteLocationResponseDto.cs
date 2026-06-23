namespace TemplateJwtProject.Models.DTOs;

public class RouteLocationResponseDto
{
    public int LocationId { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public int Order { get; set; }
    public string? Notes { get; set; }
}
