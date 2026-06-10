using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TemplateJwtProject.Constants;
using TemplateJwtProject.Data;
using TemplateJwtProject.Models;
using TemplateJwtProject.Models.DTOs;

namespace TemplateJwtProject.Controllers;

[ApiController]
[Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
public class LocationController : ControllerBase
{
    private readonly AppDbContext _context;

    public LocationController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LocationResponseDto>>> GetAll()
    {
        var locations = await _context.Locations
            .AsNoTracking()
            .OrderBy(l => l.Name)
            .Select(l => new LocationResponseDto
            {
                Id = l.Id,
                Name = l.Name,
                Description = l.Description,
                Floor = l.Floor,
                XCoordinate = l.XCoordinate,
                YCoordinate = l.YCoordinate,
                BuildingId = l.BuildingId,
                BuildingName = l.Building != null ? l.Building.Name : string.Empty
            })
            .ToListAsync();

        return Ok(locations);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LocationResponseDto>> GetById(int id)
    {
        var location = await _context.Locations
            .AsNoTracking()
            .Include(l => l.Building)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (location == null)
        {
            return NotFound(new { message = "Location not found" });
        }

        return Ok(ToResponse(location));
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<LocationResponseDto>> Create([FromBody] LocationRequestDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var buildingExists = await _context.Buildings.AnyAsync(b => b.Id == dto.BuildingId);
        if (!buildingExists)
        {
            return BadRequest(new { message = "Building does not exist" });
        }

        var entity = new Location
        {
            Name = dto.Name,
            Description = dto.Description,
            Floor = dto.Floor,
            XCoordinate = dto.XCoordinate,
            YCoordinate = dto.YCoordinate,
            BuildingId = dto.BuildingId
        };

        _context.Locations.Add(entity);
        await _context.SaveChangesAsync();

        var created = await _context.Locations
            .AsNoTracking()
            .Include(l => l.Building)
            .FirstAsync(l => l.Id == entity.Id);

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, ToResponse(created));
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<ActionResult<LocationResponseDto>> Update(int id, [FromBody] LocationRequestDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var entity = await _context.Locations.FindAsync(id);
        if (entity == null)
        {
            return NotFound(new { message = "Location not found" });
        }

        var buildingExists = await _context.Buildings.AnyAsync(b => b.Id == dto.BuildingId);
        if (!buildingExists)
        {
            return BadRequest(new { message = "Building does not exist" });
        }

        entity.Name = dto.Name;
        entity.Description = dto.Description;
        entity.Floor = dto.Floor;
        entity.XCoordinate = dto.XCoordinate;
        entity.YCoordinate = dto.YCoordinate;
        entity.BuildingId = dto.BuildingId;

        await _context.SaveChangesAsync();

        var updated = await _context.Locations
            .AsNoTracking()
            .Include(l => l.Building)
            .FirstAsync(l => l.Id == id);

        return Ok(ToResponse(updated));
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _context.Locations.FindAsync(id);
        if (entity == null)
        {
            return NotFound(new { message = "Location not found" });
        }

        var hasRoutes = await _context.RouteLocations.AnyAsync(rl => rl.LocationId == id);
        if (hasRoutes)
        {
            return BadRequest(new { message = "Location cannot be removed because routes are linked to it" });
        }

        _context.Locations.Remove(entity);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static LocationResponseDto ToResponse(Location location)
    {
        return new LocationResponseDto
        {
            Id = location.Id,
            Name = location.Name,
            Description = location.Description,
            Floor = location.Floor,
            XCoordinate = location.XCoordinate,
            YCoordinate = location.YCoordinate,
            BuildingId = location.BuildingId,
            BuildingName = location.Building?.Name ?? string.Empty
        };
    }
}
