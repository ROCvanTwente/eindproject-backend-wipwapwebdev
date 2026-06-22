using Microsoft.AspNetCore.Identity;

namespace TemplateJwtProject.Models;

public class ApplicationUser : IdentityUser
{
    public bool PasswordChanged { get; set; } = false;
    public bool RequiresAccountSetup { get; set; } = false;
    public string? FirstLoginCodeHash { get; set; }
    public DateTime? FirstLoginCodeExpiresAt { get; set; }
}
