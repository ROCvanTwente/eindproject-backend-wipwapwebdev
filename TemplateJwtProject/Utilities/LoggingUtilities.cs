namespace TemplateJwtProject.Utilities;

/// <summary>
/// Utilities for secure and consistent logging practices.
/// </summary>
public static class LoggingUtilities
{
    /// <summary>
    /// Sanitizes a value for logging by removing newline characters that could cause log injection.
    /// </summary>
    /// <param name="value">The value to sanitize.</param>
    /// <returns>Sanitized string safe for logging, or empty string if null.</returns>
    public static string SanitizeForLog(string? value)
    {
        return value?.Replace("\r", string.Empty).Replace("\n", string.Empty) ?? string.Empty;
    }

    /// <summary>
    /// Sanitizes a value for logging and handles empty/null cases with a placeholder.
    /// </summary>
    /// <param name="value">The value to sanitize.</param>
    /// <param name="placeholder">The placeholder to use if value is null or empty. Defaults to "[unknown]".</param>
    /// <returns>Sanitized string or placeholder.</returns>
    public static string SanitizeForLogOrPlaceholder(string? value, string placeholder = "[unknown]")
    {
        var sanitized = SanitizeForLog(value);
        return string.IsNullOrEmpty(sanitized) ? placeholder : sanitized;
    }

    /// <summary>
    /// Sanitizes a value for logging using its ToString() method.
    /// </summary>
    /// <param name="value">The object to sanitize.</param>
    /// <returns>Sanitized string representation, or "[null]" if value is null.</returns>
    public static string SanitizeForLog(object? value)
    {
        return value == null ? "[null]" : SanitizeForLog(value.ToString());
    }
}
