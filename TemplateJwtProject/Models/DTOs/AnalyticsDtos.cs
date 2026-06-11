using System.ComponentModel.DataAnnotations;

namespace TemplateJwtProject.Models.DTOs;

public class AnalyticsEventRequestDto
{
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
}

public class AnalyticsSummaryDto
{
    public int TotalVisitors { get; set; }
    public int TotalPageViews { get; set; }
    public int TotalRouteStarts { get; set; }
    public List<AnalyticsDailyMetricDto> DailyMetrics { get; set; } = new();
    public List<RouteStartMetricDto> RouteStarts { get; set; } = new();
    public List<PathMetricDto> PopularPages { get; set; } = new();
}

public class AnalyticsDailyMetricDto
{
    public DateTime Date { get; set; }
    public int Visitors { get; set; }
    public int PageViews { get; set; }
    public int RouteStarts { get; set; }
}

public class RouteStartMetricDto
{
    public int? RouteId { get; set; }
    public string RouteName { get; set; } = string.Empty;
    public int Starts { get; set; }
}

public class PathMetricDto
{
    public string Path { get; set; } = string.Empty;
    public int Views { get; set; }
}
