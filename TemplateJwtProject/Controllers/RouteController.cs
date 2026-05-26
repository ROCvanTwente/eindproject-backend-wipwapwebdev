using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TemplateJwtProject.Constants;
using TemplateJwtProject.Data;
using TemplateJwtProject.Models.DTOs;
using RouteEntity = TemplateJwtProject.Models.Route;
using RouteLocationEntity = TemplateJwtProject.Models.RouteLocation;

namespace TemplateJwtProject.Controllers;

[ApiController]
[Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
[Authorize(Roles = Roles.Admin)]
public class RouteController : ControllerBase
{
    private readonly AppDbContext _context;

    public RouteController(AppDbContext context)
    {
        _context = context;
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
    public async Task<ActionResult<RouteResponseDto>> Create([FromBody] RouteRequestDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var validationError = ValidateRouteLocations(dto.Locations);
        if (validationError != null)
        {
            return validationError;
        }

        var requestedLocationIds = dto.Locations.Select(l => l.LocationId).Distinct().ToList();
        var existingLocationCount = await _context.Locations.CountAsync(l => requestedLocationIds.Contains(l.Id));
        if (existingLocationCount != requestedLocationIds.Count)
        {
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

        var createdRoute = await _context.Routes
            .AsNoTracking()
            .Include(r => r.RouteLocations)
            .ThenInclude(rl => rl.Location)
            .FirstAsync(r => r.Id == route.Id);

        return CreatedAtAction(nameof(GetById), new { id = route.Id }, ToResponse(createdRoute));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<RouteResponseDto>> Update(int id, [FromBody] RouteRequestDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var validationError = ValidateRouteLocations(dto.Locations);
        if (validationError != null)
        {
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
            return BadRequest(new { message = "One or more locations do not exist" });
        }

        route.Name = dto.Name;
        route.Description = dto.Description;
        route.EstimatedTimeMinutes = dto.EstimatedTimeMinutes;

        _context.RouteLocations.RemoveRange(route.RouteLocations);

        route.RouteLocations = dto.Locations.Select(l => new RouteLocationEntity
        {
            RouteId = route.Id,
            LocationId = l.LocationId,
            Order = l.Order,
            Notes = l.Notes
        }).ToList();

        await _context.SaveChangesAsync();

        var updatedRoute = await _context.Routes
            .AsNoTracking()
            .Include(r => r.RouteLocations)
            .ThenInclude(rl => rl.Location)
            .FirstAsync(r => r.Id == id);

        return Ok(ToResponse(updatedRoute));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var route = await _context.Routes.FindAsync(id);
        if (route == null)
        {
            return NotFound(new { message = "Route not found" });
        }

        _context.Routes.Remove(route);
        await _context.SaveChangesAsync();

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
