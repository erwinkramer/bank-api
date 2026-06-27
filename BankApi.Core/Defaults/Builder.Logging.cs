using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

public static partial class ApiBuilder
{
    public static IServiceCollection AddLoggingServices(this IServiceCollection services)
    {
        var otel = services.AddOpenTelemetry();
        otel.WithLogging(logging => { }, options =>
        {
            options.IncludeFormattedMessage = true;
            options.IncludeScopes = true;
        });
        otel.WithMetrics(metrics =>
        {
            metrics.AddAspNetCoreInstrumentation();
        });
        otel.WithTracing(tracing =>
        {
            tracing.AddSource("Microsoft.AspNetCore"); //inbound http
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