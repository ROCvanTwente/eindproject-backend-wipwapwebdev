using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using TemplateJwtProject.Constants;
using TemplateJwtProject.Models;
using TemplateJwtProject.Models.DTOs;
using TemplateJwtProject.Utilities;

namespace TemplateJwtProject.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AdminController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        UserManager<ApplicationUser> userManager,
        ILogger<AdminController> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    [HttpPost("invite-admin")]
    public async Task<IActionResult> InviteAdmin([FromBody] CreateAdminInviteDto model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existingUser = await _userManager.FindByEmailAsync(model.Email);
        if (existingUser != null)
        {
            return BadRequest(new { message = "Er bestaat al een account met dit e-mailadres." });
        }

        var activationCode = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
        var activationCodeHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(activationCode)));
        var tempUserName = $"pending-{Guid.NewGuid():N}";

        var user = new ApplicationUser
        {
            UserName = tempUserName,
            Email = model.Email.Trim(),
            EmailConfirmed = true,
            PasswordChanged = false,
            RequiresAccountSetup = true,
            FirstLoginCodeHash = activationCodeHash,
            FirstLoginCodeExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        var createResult = await _userManager.CreateAsync(user);
        if (!createResult.Succeeded)
        {
            return BadRequest(new
            {
                message = "De admin-uitnodiging kon niet worden aangemaakt.",
                errors = createResult.Errors
            });
        }

        var roleResult = await _userManager.AddToRoleAsync(user, Roles.Admin);
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            return BadRequest(new
            {
                message = "De admin-rol kon niet worden gekoppeld.",
                errors = roleResult.Errors
            });
        }

        _logger.LogInformation(
            "Admin invitation created for {Email}",
            LoggingUtilities.SanitizeForLog(user.Email));

        return Ok(new
        {
            message = "Admin-uitnodiging aangemaakt.",
            email = user.Email,
            activationCode,
            activationExpiresAt = user.FirstLoginCodeExpiresAt
        });
    }

    [HttpPost("assign-role")]
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            _logger.LogWarning(
                "Assign role failed: user not found for email {Email}",
                LoggingUtilities.SanitizeForLog(model.Email));
            return NotFound(new { message = "User not found" });
        }

        // Valideer of de rol bestaat
        if (model.Role != Roles.Admin)
        {
            _logger.LogWarning(
                "Assign role failed: invalid role requested {Role} (valid role is: {ValidRole})",
                LoggingUtilities.SanitizeForLog(model.Role),
                Roles.Admin);
            return BadRequest(new { message = $"Invalid role. Valid role is: {Roles.Admin}" });
        }

        if (await _userManager.IsInRoleAsync(user, model.Role))
        {
            _logger.LogWarning(
                "Assign role failed: user {Email} already has role {Role}",
                LoggingUtilities.SanitizeForLog(model.Email),
                LoggingUtilities.SanitizeForLog(model.Role));
            return BadRequest(new { message = $"User already has the {model.Role} role" });
        }

        var result = await _userManager.AddToRoleAsync(user, model.Role);

        if (!result.Succeeded)
        {
            _logger.LogError(
                "Assign role failed: unable to add role {Role} to user {Email}. Errors: {Errors}",
                LoggingUtilities.SanitizeForLog(model.Role),
                LoggingUtilities.SanitizeForLog(model.Email),
                string.Join("; ", result.Errors.Select(e => LoggingUtilities.SanitizeForLog(e.Description))));
            return BadRequest(new { message = "Failed to assign role", errors = result.Errors });
        }

        _logger.LogInformation(
            "Admin assigned role {Role} to user {Email}",
            LoggingUtilities.SanitizeForLog(model.Role),
            LoggingUtilities.SanitizeForLog(model.Email));

        var roles = await _userManager.GetRolesAsync(user);

        // Check if it's the admin's first login
        if (model.Role == Roles.Admin && !user.PasswordChanged)
        {
            return Forbid();
        }

        return Ok(new 
        { 
            message = $"Role {model.Role} assigned successfully",
            email = user.Email,
            roles = roles
        });
    }
    [HttpPost("force-password-change")]
    public async Task<IActionResult> ForcePasswordChange([FromBody] ForcePasswordChangeDto model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            _logger.LogWarning(
                "Force password change failed: user not found for email {Email}",
                LoggingUtilities.SanitizeForLog(model.Email));
            return NotFound(new { message = "User not found" });
        }

        var newUserName = model.NewUserName.Trim();
        var existingUser = await _userManager.FindByNameAsync(newUserName);
        if (existingUser != null && existingUser.Id != user.Id)
        {
            return BadRequest(new { message = "Deze gebruikersnaam is al in gebruik." });
        }

        user.UserName = newUserName;

        if (!string.IsNullOrEmpty(user.PasswordHash))
        {
            var removePasswordResult = await _userManager.RemovePasswordAsync(user);
            if (!removePasswordResult.Succeeded)
            {
                _logger.LogError(
                    "Force password change failed: unable to remove old password for user {Email}. Errors: {Errors}",
                    LoggingUtilities.SanitizeForLog(model.Email),
                    string.Join("; ", removePasswordResult.Errors.Select(e => LoggingUtilities.SanitizeForLog(e.Description))));
                return BadRequest(new { message = "Failed to remove old password", errors = removePasswordResult.Errors });
            }
        }

        var addPasswordResult = await _userManager.AddPasswordAsync(user, model.NewPassword);
        if (!addPasswordResult.Succeeded)
        {
            _logger.LogError(
                "Force password change failed: unable to set new password for user {Email}. Errors: {Errors}",
                LoggingUtilities.SanitizeForLog(model.Email),
                string.Join("; ", addPasswordResult.Errors.Select(e => LoggingUtilities.SanitizeForLog(e.Description))));
            return BadRequest(new { message = "Failed to set new password", errors = addPasswordResult.Errors });
        }

        user.PasswordChanged = true;
        user.RequiresAccountSetup = false;
        user.FirstLoginCodeHash = null;
        user.FirstLoginCodeExpiresAt = null;
        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            _logger.LogError(
                "Force password change failed: unable to update user flag for {Email}. Errors: {Errors}",
                LoggingUtilities.SanitizeForLog(model.Email),
                string.Join("; ", updateResult.Errors.Select(e => LoggingUtilities.SanitizeForLog(e.Description))));
            return BadRequest(new { message = "Failed to update user", errors = updateResult.Errors });
        }

        _logger.LogInformation(
            "Admin forced password change for user {Email}",
            LoggingUtilities.SanitizeForLog(model.Email));

        return Ok(new 
        { 
            message = "Password changed successfully",
            email = user.Email
        });
    }


    [HttpPost("remove-role")]
    public async Task<IActionResult> RemoveRole([FromBody] AssignRoleDto model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            _logger.LogWarning(
                "Remove role failed: user not found for email {Email}",
                LoggingUtilities.SanitizeForLog(model.Email));
            return NotFound(new { message = "User not found" });
        }

        if (model.Role != Roles.Admin)
        {
            _logger.LogWarning(
                "Remove role failed: invalid role requested {Role} (valid role is: {ValidRole})",
                LoggingUtilities.SanitizeForLog(model.Role),
                Roles.Admin);
            return BadRequest(new { message = $"Invalid role. Valid role is: {Roles.Admin}" });
        }

        if (!await _userManager.IsInRoleAsync(user, model.Role))
        {
            _logger.LogWarning(
                "Remove role failed: user {Email} does not have role {Role}",
                LoggingUtilities.SanitizeForLog(model.Email),
                LoggingUtilities.SanitizeForLog(model.Role));
            return BadRequest(new { message = $"User does not have the {model.Role} role" });
        }

        var result = await _userManager.RemoveFromRoleAsync(user, model.Role);
        
        if (!result.Succeeded)
        {
            _logger.LogError(
                "Remove role failed: unable to remove role {Role} from user {Email}. Errors: {Errors}",
                LoggingUtilities.SanitizeForLog(model.Role),
                LoggingUtilities.SanitizeForLog(model.Email),
                string.Join("; ", result.Errors.Select(e => LoggingUtilities.SanitizeForLog(e.Description))));
            return BadRequest(new { message = "Failed to remove role", errors = result.Errors });
        }

        _logger.LogInformation(
            "Admin removed role {Role} from user {Email}",
            LoggingUtilities.SanitizeForLog(model.Role),
            LoggingUtilities.SanitizeForLog(model.Email));

        var roles = await _userManager.GetRolesAsync(user);
        
        return Ok(new 
        { 
            message = $"Role {model.Role} removed successfully",
            email = user.Email,
            roles = roles
        });
    }

    [HttpGet("admins")]
    public async Task<IActionResult> GetAllAdmins()
    {
        _logger.LogInformation("Admin list requested");

        var admins = await _userManager.GetUsersInRoleAsync(Roles.Admin);

        _logger.LogInformation("Retrieved {AdminCount} admins", admins.Count);

        var adminList = new List<object>();

        foreach (var admin in admins)
        {
            var roles = await _userManager.GetRolesAsync(admin);
            adminList.Add(new
            {
                id = admin.Id,
                email = admin.Email,
                userName = admin.UserName,
                roles = roles,
                requiresAccountSetup = admin.RequiresAccountSetup
            });
        }

        return Ok(adminList);
    }

}
