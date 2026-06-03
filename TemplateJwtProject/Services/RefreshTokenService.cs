using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using TemplateJwtProject.Data;
using TemplateJwtProject.Models;
using TemplateJwtProject.Utilities;

namespace TemplateJwtProject.Services;

public interface IRefreshTokenService
{
    Task<RefreshToken> GenerateRefreshTokenAsync(string userId);
    Task<RefreshToken?> ValidateRefreshTokenAsync(string token);
    Task RevokeRefreshTokenAsync(string token, string reason);
    Task RevokeAllUserRefreshTokensAsync(string userId);
    Task CleanupExpiredTokensAsync();
}

public class RefreshTokenService : IRefreshTokenService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RefreshTokenService> _logger;

    public RefreshTokenService(AppDbContext context, IConfiguration configuration, ILogger<RefreshTokenService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<RefreshToken> GenerateRefreshTokenAsync(string userId)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var expiryInDays = int.Parse(jwtSettings["RefreshTokenExpiryInDays"] ?? "7");

        var refreshToken = new RefreshToken
        {
            Token = GenerateSecureToken(),
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(expiryInDays),
            CreatedAt = DateTime.UtcNow
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        _logger.LogDebug("Refresh token generated for user {UserId}, expires in {ExpiryDays} days", 
            LoggingUtilities.SanitizeForLog(userId), expiryInDays);

        return refreshToken;
    }

    public async Task<RefreshToken?> ValidateRefreshTokenAsync(string token)
    {
        var refreshToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token);

        if (refreshToken == null)
        {
            _logger.LogWarning("Refresh token validation failed: token not found");
            return null;
        }

        if (!refreshToken.IsActive)
        {
            var reason = refreshToken.IsRevoked ? "revoked" : "expired";
            _logger.LogWarning("Refresh token validation failed: token is {Reason}", reason);
            return null;
        }

        return refreshToken;
    }

    public async Task RevokeRefreshTokenAsync(string token, string reason)
    {
        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token);

        if (refreshToken == null)
        {
            _logger.LogWarning("Revoke refresh token failed: token not found");
            return;
        }

        if (!refreshToken.IsActive)
        {
            _logger.LogWarning("Revoke refresh token failed: token is already revoked or expired");
            return;
        }

        refreshToken.RevokedAt = DateTime.UtcNow;
        refreshToken.ReasonRevoked = LoggingUtilities.SanitizeForLog(reason);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Refresh token revoked. Reason: {Reason}", 
            LoggingUtilities.SanitizeForLog(reason));
    }

    public async Task RevokeAllUserRefreshTokensAsync(string userId)
    {
        var activeTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();

        if (activeTokens.Count == 0)
        {
            _logger.LogDebug("No active refresh tokens to revoke for user {UserId}", 
                LoggingUtilities.SanitizeForLog(userId));
            return;
        }

        foreach (var token in activeTokens)
        {
            token.RevokedAt = DateTime.UtcNow;
            token.ReasonRevoked = "User requested logout from all devices";
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Revoked {TokenCount} refresh tokens for user {UserId} (logout from all devices)", 
            activeTokens.Count, 
            LoggingUtilities.SanitizeForLog(userId));
    }

    public async Task CleanupExpiredTokensAsync()
    {
        var expiredTokens = await _context.RefreshTokens
            .Where(rt => rt.ExpiresAt < DateTime.UtcNow || rt.RevokedAt != null)
            .ToListAsync();

        if (expiredTokens.Count == 0)
        {
            _logger.LogDebug("No expired or revoked refresh tokens to clean up");
            return;
        }

        _context.RefreshTokens.RemoveRange(expiredTokens);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Cleanup: removed {ExpiredTokenCount} expired or revoked refresh tokens", 
            expiredTokens.Count);
    }

    private static string GenerateSecureToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
