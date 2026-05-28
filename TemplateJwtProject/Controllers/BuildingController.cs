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
// [Authorize(Roles = Roles.Admin)]
public class BuildingController : ControllerBase
{
    private readonly AppDbContext _context;

    public BuildingController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BuildingResponseDto>>> GetAll()
    {
        var buildings = await _context.Buildings
            .AsNoTracking()
            .OrderBy(b => b.Name)
            .Select(b => new BuildingResponseDto
            {
                Id = b.Id,
                Name = b.Name,
                Description = b.Description
            })
            .ToListAsync();

        return Ok(buildings);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BuildingResponseDto>> GetById(int id)
    {
        var building = await _context.Buildings
            .AsNoTracking()
            .Where(b => b.Id == id)
            .Select(b => new BuildingResponseDto
            {
                Id = b.Id,
                Name = b.Name,
                Description = b.Description
            })
            .FirstOrDefaultAsync();

        if (building == null)
        {
            return NotFound(new { message = "Building not found" });
        }

        return Ok(building);
    }

    [HttpPost]
    public async Task<ActionResult<BuildingResponseDto>> Create([FromBody] BuildingRequestDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var entity = new Building
        {
            Name = dto.Name,
            Description = dto.Description
        };

        _context.Buildings.Add(entity);
        await _context.SaveChangesAsync();

        var response = new BuildingResponseDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description
        };

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, response);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<BuildingResponseDto>> Update(int id, [FromBody] BuildingRequestDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var entity = await _context.Buildings.FindAsync(id);
        if (entity == null)
        {
            return NotFound(new { message = "Building not found" });
        }

        entity.Name = dto.Name;
        entity.Description = dto.Description;

        await _context.SaveChangesAsync();

        return Ok(new BuildingResponseDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description
        });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _context.Buildings.FindAsync(id);
        if (entity == null)
        {
            return NotFound(new { message = "Building not found" });
        }

        var hasLocations = await _context.Locations.AnyAsync(l => l.BuildingId == id);
        if (hasLocations)
        {
            return BadRequest(new { message = "Building cannot be removed because locations are linked to it" });
        }

        _context.Buildings.Remove(entity);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
