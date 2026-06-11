using System.ComponentModel.DataAnnotations;

namespace TemplateJwtProject.Models;

public class AnalyticsEvent
{
    public int Id { get; set; }

    [Required]
    [MaxLength(40)]
    public string EventType { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Path { get; set; }

    public int? RouteId { get; set; }

    [MaxLength(100)]
    public string? RouteName { get; set; }

    [MaxLength(80)]
    public string? VisitorId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
