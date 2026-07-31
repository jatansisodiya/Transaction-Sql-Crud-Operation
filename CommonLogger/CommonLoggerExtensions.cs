using CommonLogger;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;

namespace Microsoft.Extensions.DependencyInjection;

public static class CommonLoggerExtensions
{
    /// <summary>
    /// Registers CommonLogger service with Dependency Injection, HttpContextAccessor, and initializes static telemetry.
    /// </summary>
    public static IServiceCollection AddCommonLogger(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHttpContextAccessor();

        services.AddSingleton<IAILogger>(sp =>
        {
            var telemetryClient = sp.GetService<TelemetryClient>();
            var httpContextAccessor = sp.GetService<IHttpContextAccessor>();
            var logger = new AILogger(telemetryClient, httpContextAccessor);
            if (telemetryClient != null)
            {
                AILogger.ConfigureStaticTelemetry(telemetryClient, httpContextAccessor);
            }
            return logger;
        });

        return services;
    }
}
