using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;

namespace CommonLogger;

public class CommonLogger : ICommonLogger
{
    private readonly TelemetryClient? _telemetryClient;
    private readonly IHttpContextAccessor? _httpContextAccessor;
    private static TelemetryClient? _staticTelemetryClient;
    private static IHttpContextAccessor? _staticHttpContextAccessor;
    private static readonly object FileLock = new();

    public CommonLogger(TelemetryClient? telemetryClient = null, IHttpContextAccessor? httpContextAccessor = null)
    {
        _telemetryClient = telemetryClient;
        _httpContextAccessor = httpContextAccessor;
        if (telemetryClient != null)
        {
            _staticTelemetryClient = telemetryClient;
        }
        if (httpContextAccessor != null)
        {
            _staticHttpContextAccessor = httpContextAccessor;
        }
    }

    /// <summary>
    /// Master toggle for enabling or disabling AI telemetry and file logging.
    /// When set to true, AI logging runs. When false, no logs are saved or sent.
    /// </summary>
    public static bool IsLoggingEnabled { get; set; } = false;
    
    private static readonly System.Collections.Concurrent.ConcurrentBag<string> IgnoredApiUrls = new();
    
    /// <summary>
    /// Configures the static TelemetryClient and HttpContextAccessor for non-DI static logger calls.
    /// </summary>
    public static void ConfigureStaticTelemetry(TelemetryClient telemetryClient, IHttpContextAccessor? httpContextAccessor = null)
    {
        _staticTelemetryClient = telemetryClient;
        if (httpContextAccessor != null)
        {
            _staticHttpContextAccessor = httpContextAccessor;
        }
    }

    public void LogError(
        Exception exception,
        string? message = null,
        object? parameterValues = null,
        IDictionary<string, string>? customProperties = null)
    {
        LogErrorStatic(exception, message, parameterValues, customProperties, _telemetryClient ?? _staticTelemetryClient, _httpContextAccessor ?? _staticHttpContextAccessor);
    }

    public void LogWarning(
        string message,
        object? parameterValues = null,
        IDictionary<string, string>? customProperties = null)
    {
        LogTraceStatic(message, SeverityLevel.Warning, parameterValues, customProperties, _telemetryClient ?? _staticTelemetryClient, _httpContextAccessor ?? _staticHttpContextAccessor);
    }

    public void LogInformation(
        string message,
        object? parameterValues = null,
        IDictionary<string, string>? customProperties = null)
    {
        LogTraceStatic(message, SeverityLevel.Information, parameterValues, customProperties, _telemetryClient ?? _staticTelemetryClient, _httpContextAccessor ?? _staticHttpContextAccessor);
    }

    public void LogTrace(
        string message,
        object? parameterValues = null,
        IDictionary<string, string>? customProperties = null)
    {
        LogTraceStatic(message, SeverityLevel.Verbose, parameterValues, customProperties, _telemetryClient ?? _staticTelemetryClient, _httpContextAccessor ?? _staticHttpContextAccessor);
    }

    #region Static Helper Methods

    public static void LogMessage(string message)
    {
        var caller = ExtractCallerFrame();
        var props = BuildPropertiesDictionary(null, null, caller.memberName, caller.filePath, caller.lineNumber, _staticHttpContextAccessor);
        //LogFileOnly("INFO", message, props, null, caller.memberName, caller.filePath, caller.lineNumber);
    }

    public static void LogErrorStatic(
        Exception exception,
        string? message = null,
        object? parameterValues = null,
        IDictionary<string, string>? customProperties = null,
        TelemetryClient? telemetry = null,
        IHttpContextAccessor? httpContextAccessor = null)
    {
        if (!IsLoggingEnabled)
        {
            return;
        }

        var accessor = httpContextAccessor ?? _staticHttpContextAccessor;
        if (accessor?.HttpContext != null && IsUrlIgnored(accessor.HttpContext.Request.Path))
        {
            return;
        }

        var client = telemetry ?? _staticTelemetryClient;
        var caller = ExtractCallerFrame(exception);
        
        var properties = BuildPropertiesDictionary(parameterValues, customProperties, caller.memberName, caller.filePath, caller.lineNumber, accessor);
        
        string logMsg = message ?? exception.Message;
        properties["ErrorMessage"] = exception.Message;
        properties["ExceptionType"] = exception.GetType().FullName ?? exception.GetType().Name;
        if (exception.StackTrace != null)
        {
            properties["StackTrace"] = exception.StackTrace;
        }

        if (client != null)
        {
            try
            {
                client.TrackException(exception, properties);
                client.Flush();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CommonLogger] Failed to send exception telemetry to Application Insights: {ex.Message}");
            }
        }

        // Local file logging fallback / backup
        //LogFileOnly("ERROR", $"{logMsg} | Exception: {exception.Message}\nStackTrace: {exception.StackTrace}", properties, exception, caller.memberName, caller.filePath, caller.lineNumber);
    }

    public static void LogTraceStatic(
        string message,
        SeverityLevel severityLevel,
        object? parameterValues = null,
        IDictionary<string, string>? customProperties = null,
        TelemetryClient? telemetry = null,
        IHttpContextAccessor? httpContextAccessor = null)
    {
        if (!IsLoggingEnabled)
        {
            return;
        }

        var accessor = httpContextAccessor ?? _staticHttpContextAccessor;
        if (accessor?.HttpContext != null && IsUrlIgnored(accessor.HttpContext.Request.Path))
        {
            return;
        }

        var client = telemetry ?? _staticTelemetryClient;
        var caller = ExtractCallerFrame();
        
        var properties = BuildPropertiesDictionary(parameterValues, customProperties, caller.memberName, caller.filePath, caller.lineNumber, accessor);

        if (client != null)
        {
            try
            {
                client.TrackTrace(message, severityLevel, properties);
                client.Flush();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CommonLogger] Failed to send trace telemetry to Application Insights: {ex.Message}");
            }
        }

        //LogFileOnly(severityLevel.ToString().ToUpper(), message, properties, null, caller.memberName, caller.filePath, caller.lineNumber);
    }


    /// <summary>
    /// Configures whether logging is enabled or disabled globally.
    /// </summary>
    public static void SetLoggingEnabled(bool enabled)
    {
        IsLoggingEnabled = enabled;
    }


    /// <summary>
    /// Registers one or more API URL paths/prefixes to be ignored from logging (e.g. "/health", "/swagger", "/favicon.ico").
    /// </summary>
    public static void IgnoreApiUrl(params string[] urlPaths)
    {
        if (urlPaths == null) return;
        foreach (var path in urlPaths)
        {
            if (!string.IsNullOrWhiteSpace(path) && !IgnoredApiUrls.Contains(path, StringComparer.OrdinalIgnoreCase))
            {
                IgnoredApiUrls.Add(path);
            }
        }
    }

    /// <summary>
    /// Checks if a given request path matches any of the registered ignored API URLs/prefixes.
    /// </summary>
    public static bool IsUrlIgnored(string? requestPath)
    {
        if (string.IsNullOrWhiteSpace(requestPath)) return false;
        foreach (var ignoredPath in IgnoredApiUrls)
        {
            if (requestPath.StartsWith(ignoredPath, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Safely reads the HttpRequest body as a string without breaking downstream reading or model binding.
    /// </summary>
    public static async Task<string> ReadRequestBodyAsync(HttpRequest request)
    {
        if (request == null || request.ContentLength == null || request.ContentLength == 0 || !request.Body.CanRead)
        {
            return string.Empty;
        }

        try
        {
            request.EnableBuffering();
            request.Body.Position = 0;
            using var reader = new System.IO.StreamReader(request.Body, System.Text.Encoding.UTF8, leaveOpen: true);
            string bodyText = await reader.ReadToEndAsync();
            request.Body.Position = 0;
            return bodyText;
        }
        catch
        {
            return "[Error Reading Body]";
        }
    }
    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Dynamically inspects the StackTrace to find the exact calling method, file name, and line number outside of CommonLogger.
    /// </summary>
    private static (string memberName, string filePath, int lineNumber) ExtractCallerFrame(Exception? exception = null)
    {
        if (exception != null)
        {
            var exStackTrace = new StackTrace(exception, true);
            var frames = exStackTrace.GetFrames();
            if (frames != null && frames.Length > 0)
            {
                foreach (var frame in frames)
                {
                    var method = frame.GetMethod();
                    if (method == null) continue;
                    var declaringType = method.DeclaringType;
                    
                    if (declaringType != null && declaringType.Assembly == typeof(CommonLogger).Assembly)
                        continue;

                    string mName = declaringType != null ? $"{declaringType.Name}.{method.Name}" : method.Name;
                    string fPath = frame.GetFileName() ?? "";
                    int lNum = frame.GetFileLineNumber();
                    return (mName, fPath, lNum);
                }
            }
        }

        var stackTrace = new StackTrace(true);
        var activeFrames = stackTrace.GetFrames();
        if (activeFrames != null)
        {
            foreach (var frame in activeFrames)
            {
                var method = frame.GetMethod();
                if (method == null) continue;
                var declaringType = method.DeclaringType;

                if (declaringType != null && (declaringType.Assembly == typeof(CommonLogger).Assembly || declaringType.FullName?.StartsWith("CommonLogger") == true))
                {
                    continue;
                }

                string mName = declaringType != null ? $"{declaringType.Name}.{method.Name}" : method.Name;
                string fPath = frame.GetFileName() ?? "";
                int lNum = frame.GetFileLineNumber();
                return (mName, fPath, lNum);
            }
        }

        return ("UnknownMethod", "", 0);
    }

    private static Dictionary<string, string> BuildPropertiesDictionary(
        object? parameterValues,
        IDictionary<string, string>? customProperties,
        string memberName,
        string filePath,
        int lineNumber,
        IHttpContextAccessor? httpContextAccessor = null)
    {
        var props = customProperties != null 
            ? new Dictionary<string, string>(customProperties) 
            : new Dictionary<string, string>();

        if (!props.ContainsKey("LogType"))
        {
            props["LogType"] = "Server";
        }

        if (!string.IsNullOrEmpty(memberName))
        {
            props["MethodName"] = memberName;
        }

        if (!string.IsNullOrEmpty(filePath))
        {
            props["FileName"] = Path.GetFileName(filePath);
            props["FilePath"] = filePath;
        }

        if (lineNumber > 0)
        {
            props["LineNumber"] = lineNumber.ToString();
        }

        // Dynamically capture HTTP Request Origin Metadata if inside an API request context
        var context = httpContextAccessor?.HttpContext;
        if (context != null)
        {
            var req = context.Request;

            // 1. Client IP Address (handling proxies like X-Forwarded-For)
            string clientIp = req.Headers["X-Forwarded-For"].FirstOrDefault() 
                              ?? context.Connection.RemoteIpAddress?.ToString() 
                              ?? "Unknown IP";
            props["ClientIP"] = clientIp;

            // 2. User-Agent (Browser, Postman, Mobile App, Swagger, etc.)
            string userAgent = req.Headers["User-Agent"].ToString();
            if (!string.IsNullOrEmpty(userAgent))
            {
                props["UserAgent"] = userAgent;
            }

            // 3. Referer (The web page URL where the API call originated)
            string referer = req.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer))
            {
                props["Referer"] = referer;
            }

            // 4. Origin (Domain/Origin of caller website)
            string origin = req.Headers["Origin"].ToString();
            if (!string.IsNullOrEmpty(origin))
            {
                props["Origin"] = origin;
            }

            // 5. Target Full Request URL, Path & HTTP Method
            string fullUrl = $"{req.Scheme}://{req.Host}{req.Path}{req.QueryString}";
            props["FullUrl"] = fullUrl;
            props["RequestMethod"] = req.Method;
            props["RequestPath"] = req.Path;
            props["QueryString"] = req.QueryString.ToString();

            // 6. User Identity / Claims (if authenticated)
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                props["UserIdentity"] = context.User.Identity.Name ?? "AuthenticatedUser";
            }
        }

        if (parameterValues != null)
        {
            try
            {
                string jsonParams = parameterValues is string strVal 
                    ? strVal 
                    : JsonSerializer.Serialize(parameterValues, new JsonSerializerOptions { WriteIndented = false });
                props["ParameterValues"] = jsonParams;
            }
            catch (Exception ex)
            {
                props["ParameterValues"] = $"[Serialization Error: {ex.Message}]";
            }
        }

        return props;
    }

    private static void LogFileOnly(
        string level,
        string message,
        Dictionary<string, string>? properties,
        Exception? exception,
        string memberName,
        string filePath,
        int lineNumber)
    {
        string logDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string logFilePath = Path.Combine(logDirectory, "application.log");

        try
        {
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string callerInfo = !string.IsNullOrEmpty(memberName) 
                ? $"[{Path.GetFileName(filePath)}:{memberName}():L{lineNumber}] " 
                : "";

            string originInfo = "";
            if (properties != null)
            {
                string ip = properties.TryGetValue("ClientIP", out var ipVal) ? ipVal : null;
                string ua = properties.TryGetValue("UserAgent", out var uaVal) ? uaVal : null;
                string refUrl = properties.TryGetValue("Referer", out var refVal) ? refVal : null;

                if (ip != null || ua != null || refUrl != null)
                {
                    originInfo = $" | Origin: [IP: {ip ?? "N/A"}, UserAgent: {ua ?? "N/A"}, Referer: {refUrl ?? "Direct/N/A"}]";
                }
            }

            string paramsInfo = properties != null && properties.TryGetValue("ParameterValues", out var pVal) 
                ? $" | Parameters: {pVal}" 
                : "";

            string logEntry = $"[{timestamp}] [{level}] {callerInfo}{message}{originInfo}{paramsInfo}" + Environment.NewLine;

            lock (FileLock)
            {
                File.AppendAllText(logFilePath, logEntry);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CommonLogger] Error writing to log file: {ex.Message}");
        }
    }

    #endregion
}
