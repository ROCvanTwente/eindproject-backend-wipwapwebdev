namespace TemplateJwtProject.Utilities;

public static class LoggingUtilities
{
    public static string SanitizeForLog(string? value)
    {
        return value?.Replace("\r", string.Empty).Replace("\n", string.Empty) ?? string.Empty;
    }
}
