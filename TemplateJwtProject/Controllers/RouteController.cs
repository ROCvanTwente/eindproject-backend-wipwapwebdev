using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TemplateJwtProject.Constants;
using TemplateJwtProject.Data;
using TemplateJwtProject.Models.DTOs;
using TemplateJwtProject.Utilities;
using RouteEntity = TemplateJwtProject.Models.Route;
using RouteLocationEntity = TemplateJwtProject.Models.RouteLocation;

namespace TemplateJwtProject.Controllers;

[ApiController]
[Microsoft.AspNetCore.Mvc.Route("api/routes")]
public class RouteController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<RouteController> _logger;

    public RouteController(AppDbContext context, ILogger<RouteController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RouteResponseDto>>> GetAll()
    {
        var routes = await _context.Routes
            .AsNoTracking()
            .Include(r => r.RouteLocations)
            .ThenInclude(rl => rl.Location)
            .OrderBy(r => r.Name)
            .ToListAsync();

        return Ok(routes.Select(ToResponse));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RouteResponseDto>> GetById(int id)
    {
        var route = await _context.Routes
            .AsNoTracking()
            .Include(r => r.RouteLocations)
            .ThenInclude(rl => rl.Location)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (route == null)
        {
            return NotFound(new { message = "Route not found" });
        }

        return Ok(ToResponse(route));
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<RouteResponseDto>> Create([FromBody] RouteRequestDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var validationError = ValidateRouteLocations(dto.Locations);
        if (validationError != null)
        {
            _logger.LogWarning("Route creation failed: validation error in locations");
            return validationError;
        }

        var requestedLocationIds = dto.Locations.Select(l => l.LocationId).Distinct().ToList();
        var existingLocationCount = await _context.Locations.CountAsync(l => requestedLocationIds.Contains(l.Id));
        if (existingLocationCount != requestedLocationIds.Count)
        {
            _logger.LogWarning("Route creation failed: one or more locations do not exist");
            return BadRequest(new { message = "One or more locations do not exist" });
        }

        var route = new RouteEntity
        {
            Name = dto.Name,
            Description = dto.Description,
            EstimatedTimeMinutes = dto.EstimatedTimeMinutes,
            RouteLocations = dto.Locations.Select(l => new RouteLocationEntity
            {
                LocationId = l.LocationId,
                Order = l.Order,
                Notes = l.Notes
            }).ToList()
        };

        _context.Routes.Add(route);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Route created: {RouteId} - {RouteName} with {LocationCount} locations", 
            route.Id, 
            LoggingUtilities.SanitizeForLog(route.Name),
            route.RouteLocations.Count);

        var createdRoute = await _context.Routes
            .AsNoTracking()
            .Include(r => r.RouteLocations)
            .ThenInclude(rl => rl.Location)
            .FirstAsync(r => r.Id == route.Id);

        return CreatedAtAction(nameof(GetById), new { id = route.Id }, ToResponse(createdRoute));
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<ActionResult<RouteResponseDto>> Update(int id, [FromBody] RouteRequestDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var validationError = ValidateRouteLocations(dto.Locations);
        if (validationError != null)
        {
            _logger.LogWarning("Route update failed: validation error in locations for route {RouteId}", id);
            return validationError;
        }

        var route = await _context.Routes
            .Include(r => r.RouteLocations)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (route == null)
        {
            return NotFound(new { message = "Route not found" });
        }

        var requestedLocationIds = dto.Locations.Select(l => l.LocationId).Distinct().ToList();
        var existingLocationCount = await _context.Locations.CountAsync(l => requestedLocationIds.Contains(l.Id));
        if (existingLocationCount != requestedLocationIds.Count)
        {
            _logger.LogWarning("Route update failed: one or more locations do not exist for route {RouteId}", id);
            return BadRequest(new { message = "One or more locations do not exist" });
        }

        var oldName = route.Name;
        var oldLocationCount = route.RouteLocations.Count;

        var updatedRouteLocations = dto.Locations.Select(l => new RouteLocationEntity
        {
            RouteId = route.Id,
            LocationId = l.LocationId,
            Order = l.Order,
            Notes = l.Notes
        }).ToList();

        var executionStrategy = _context.Database.CreateExecutionStrategy();
        await executionStrategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            route.Name = dto.Name;
            route.Description = dto.Description;
            route.EstimatedTimeMinutes = dto.EstimatedTimeMinutes;

            _context.RouteLocations.RemoveRange(route.RouteLocations);
            await _context.SaveChangesAsync();

            _context.RouteLocations.AddRange(updatedRouteLocations);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
        });

        route.RouteLocations = updatedRouteLocations;

        _logger.LogInformation("Route updated: {RouteId} - renamed from {OldName} to {NewName}, locations changed from {OldLocationCount} to {NewLocationCount}", 
            id, 
            LoggingUtilities.SanitizeForLog(oldName), 
            LoggingUtilities.SanitizeForLog(route.Name),
            oldLocationCount,
            route.RouteLocations.Count);

        var updatedRoute = await _context.Routes
            .AsNoTracking()
            .Include(r => r.RouteLocations)
            .ThenInclude(rl => rl.Location)
            .FirstAsync(r => r.Id == id);

        return Ok(ToResponse(updatedRoute));
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var route = await _context.Routes.FindAsync(id);
        if (route == null)
        {
            return NotFound(new { message = "Route not found" });
        }

        _context.Routes.Remove(route);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Route deleted: {RouteId} - {RouteName}", 
            id, 
            LoggingUtilities.SanitizeForLog(route.Name));

        return NoContent();
    }

    private static RouteResponseDto ToResponse(RouteEntity route)
    {
        return new RouteResponseDto
        {
            Id = route.Id,
            Name = route.Name,
            Description = route.Description,
            EstimatedTimeMinutes = route.EstimatedTimeMinutes,
            Locations = route.RouteLocations
                .OrderBy(rl => rl.Order)
                .Select(rl => new RouteLocationResponseDto
                {
                    LocationId = rl.LocationId,
                    LocationName = rl.Location?.Name ?? string.Empty,
                    Order = rl.Order,
                    Notes = rl.Notes
                })
                .ToList()
        };
    }

    private static BadRequestObjectResult? ValidateRouteLocations(IEnumerable<RouteLocationRequestDto> routeLocations)
    {
        var routeLocationsList = routeLocations.ToList();

        if (routeLocationsList.Count == 0)
        {
            return new BadRequestObjectResult(new { message = "A route must contain at least one location" });
        }

        if (routeLocationsList.Select(l => l.LocationId).Distinct().Count() != routeLocationsList.Count)
        {
            return new BadRequestObjectResult(new { message = "A route cannot contain duplicate locations" });
        }

        if (routeLocationsList.Select(l => l.Order).Distinct().Count() != routeLocationsList.Count)
        {
            return new BadRequestObjectResult(new { message = "Each route location must have a unique order value" });
        }

        return null;
    }
}
