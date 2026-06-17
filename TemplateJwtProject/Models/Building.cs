using System.ComponentModel.DataAnnotations;

namespace TemplateJwtProject.Models;

public class Building
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(20000)]
    public string Description { get; set; } = string.Empty;

    public ICollection<Location> Locations { get; set; } = new List<Location>();
}
