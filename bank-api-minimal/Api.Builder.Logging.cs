using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

static partial class ApiBuilder
{
    public static IServiceCollection AddLoggingServices(this IServiceCollection services)
    {
        var otel = services.AddOpenTelemetry();
        otel.WithMetrics(metrics =>
        {
            metrics.AddAspNetCoreInstrumentation();
            metrics.AddMeter("Microsoft.AspNetCore.Hosting");
            metrics.AddMeter("Microsoft.AspNetCore.Server.Kestrel");
        });
        otel.WithTracing(tracing =>
        {
            tracing.AddAspNetCoreInstrumentation(); //inbound http
            tracing.AddHttpClientInstrumentation(); //outbound http
            tracing.AddEntityFrameworkCoreInstrumentation();
        });
        otel.UseOtlpExporter();

        services.AddTransient<ILogger>(p =>
        {
            return p.GetRequiredService<ILoggerFactory>().CreateLogger("API logger");
        });

        return services;
    }
}