using System.Linq;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace CommonLogger;

public class GlobalExceptionHandler(ICommonLogger logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var request = httpContext.Request;

        // Skip logging if logging is disabled or request URL is registered in ignored list
        if (!CommonLogger.IsLoggingEnabled || CommonLogger.IsUrlIgnored(request.Path))
        {
            return false;
        }

        // Extract route & query info for telemetry properties
        var routeData = httpContext.GetRouteData();
        string controllerName = routeData.Values["controller"]?.ToString() ?? "UnknownController";
        string actionName = routeData.Values["action"]?.ToString() ?? request.Path;

        string clientIp = request.Headers["X-Forwarded-For"].FirstOrDefault() 
                          ?? httpContext.Connection.RemoteIpAddress?.ToString() 
                          ?? "Unknown IP";

        // Read request body safely as string
        string requestBody = await CommonLogger.ReadRequestBodyAsync(request);

        string referer = request.Headers["Referer"].ToString();
        string origin = request.Headers["Origin"].ToString();
        string fullUrl = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";

        var customProperties = new Dictionary<string, string>
        {
            ["HttpProtocol"] = request.Protocol,
            ["HttpMethod"] = request.Method,
            ["RequestPath"] = request.Path,
            ["FullUrl"] = fullUrl,
            ["QueryString"] = request.QueryString.ToString(),
            ["RequestBody"] = requestBody,
            ["Controller"] = controllerName,
            ["Action"] = actionName,
            ["ClientIP"] = clientIp,
            ["UserAgent"] = request.Headers["User-Agent"].ToString(),
            ["Referer"] = referer,
            ["Origin"] = origin
        };

        var parameterValues = new
        {
            Path = request.Path.Value,
            Method = request.Method,
            Query = request.QueryString.Value,
            ClientIp = clientIp,
            UserAgent = request.Headers["User-Agent"].ToString(),
            Referer = request.Headers["Referer"].ToString(),
            RouteValues = routeData.Values
        };

        // Log exception with stack trace, parameters, and custom properties to Application Insights & local file
        logger.LogError(
            exception: exception,
            message: $"Unhandled exception in {request.Method} {request.Path}",
            parameterValues: parameterValues,
            customProperties: customProperties
        );

        // Prepare standard ProblemDetails response
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An error occurred while processing your request.",
            Detail = exception.Message,
            Instance = request.Path
        };

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
