namespace CommonLogger;

public interface IAILogger
{
    /// <summary>
    /// Logs an exception with stack trace, dynamically resolved caller method name, file path, line number, and method parameter values to Application Insights and local file.
    /// </summary>
    void LogError(
        Exception exception,
        string? message = null,
        object? parameterValues = null,
        IDictionary<string, string>? customProperties = null);

    /// <summary>
    /// Logs a warning message with dynamically resolved caller method name, file path, line number, and method parameter values.
    /// </summary>
    void LogWarning(
        string message,
        object? parameterValues = null,
        IDictionary<string, string>? customProperties = null);

    /// <summary>
    /// Logs an informational message with dynamically resolved caller method name, file path, line number, and method parameter values.
    /// </summary>
    void LogInformation(
        string message,
        object? parameterValues = null,
        IDictionary<string, string>? customProperties = null);

    /// <summary>
    /// Logs a trace message with dynamically resolved caller method name, file path, line number, and method parameter values.
    /// </summary>
    void LogTrace(
        string message,
        object? parameterValues = null,
        IDictionary<string, string>? customProperties = null);
}
