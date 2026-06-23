using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TemplateJwtProject.Data;
using TemplateJwtProject.Models;
using TemplateJwtProject.Models.DTOs;

namespace TemplateJwtProject.Controllers;

[ApiController]
[Route("api/analytics")]
public class AnalyticsController : ControllerBase
{
    private const string PageViewEvent = "page_view";
    private const string RouteStartEvent = "route_start";
    private readonly AppDbContext _context;

    public AnalyticsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("events")]
    public async Task<IActionResult> TrackEvent([FromBody] AnalyticsEventRequestDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (dto.EventType != PageViewEvent && dto.EventType != RouteStartEvent)
        {
            return BadRequest(new { message = "Unsupported analytics event type" });
        }

        var analyticsEvent = new AnalyticsEvent
        {
            EventType = dto.EventType,
            Path = string.IsNullOrWhiteSpace(dto.Path) ? null : dto.Path.Trim(),
            RouteId = dto.RouteId,
            RouteName = string.IsNullOrWhiteSpace(dto.RouteName) ? null : dto.RouteName.Trim(),
            VisitorId = string.IsNullOrWhiteSpace(dto.VisitorId) ? null : dto.VisitorId.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _context.AnalyticsEvents.Add(analyticsEvent);
        await _context.SaveChangesAsync();

        return Accepted();
    }

    [HttpGet("summary")]
    [Authorize]
    public async Task<ActionResult<AnalyticsSummaryDto>> GetSummary()
    {
        var since = DateTime.UtcNow.Date.AddDays(-29);

        var events = await _context.AnalyticsEvents
            .AsNoTracking()
            .Where(e => e.CreatedAt >= since)
            .ToListAsync();

        var routeNames = await _context.Routes
            .AsNoTracking()
            .ToDictionaryAsync(route => route.Id, route => route.Name);

        var dailyMetrics = Enumerable.Range(0, 30)
            .Select(offset =>
            {
                var date = since.AddDays(offset);
                var dayEvents = events.Where(e => e.CreatedAt.Date == date).ToList();

                return new AnalyticsDailyMetricDto
                {
                    Date = date,
                    Visitors = dayEvents
                        .Where(e => e.EventType == PageViewEvent && !string.IsNullOrWhiteSpace(e.VisitorId))
                        .Select(e => e.VisitorId)
                        .Distinct()
                        .Count(),
                    PageViews = dayEvents.Count(e => e.EventType == PageViewEvent),
                    RouteStarts = dayEvents.Count(e => e.EventType == RouteStartEvent)
                };
            })
            .ToList();

        var routeStarts = events
            .Where(e => e.EventType == RouteStartEvent)
            .GroupBy(e => new { e.RouteId, e.RouteName })
            .Select(group => new RouteStartMetricDto
            {
                RouteId = group.Key.RouteId,
                RouteName = ResolveRouteName(group.Key.RouteId, group.Key.RouteName, routeNames),
                Starts = group.Count()
            })
            .OrderByDescending(metric => metric.Starts)
            .ThenBy(metric => metric.RouteName)
            .Take(10)
            .ToList();

        var popularPages = events
            .Where(e => e.EventType == PageViewEvent && !string.IsNullOrWhiteSpace(e.Path))
            .GroupBy(e => e.Path!)
            .Select(group => new PathMetricDto
            {
                Path = group.Key,
                Views = group.Count()
            })
            .OrderByDescending(metric => metric.Views)
            .ThenBy(metric => metric.Path)
            .Take(10)
            .ToList();

        return Ok(new AnalyticsSummaryDto
        {
            TotalVisitors = events
                .Where(e => e.EventType == PageViewEvent && !string.IsNullOrWhiteSpace(e.VisitorId))
                .Select(e => e.VisitorId)
                .Distinct()
                .Count(),
            TotalPageViews = events.Count(e => e.EventType == PageViewEvent),
            TotalRouteStarts = events.Count(e => e.EventType == RouteStartEvent),
            DailyMetrics = dailyMetrics,
            RouteStarts = routeStarts,
            PopularPages = popularPages
        });
    }

    private static string ResolveRouteName(int? routeId, string? eventRouteName, IReadOnlyDictionary<int, string> routeNames)
    {
        if (routeId.HasValue && routeNames.TryGetValue(routeId.Value, out var currentRouteName))
        {
            return currentRouteName;
        }

        return string.IsNullOrWhiteSpace(eventRouteName) ? "Onbekende route" : eventRouteName;
    }
}
