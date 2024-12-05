using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

public static partial class ApiBuilder
{
    public static IHostApplicationBuilder AddLoggingServices(this IHostApplicationBuilder builder)
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        var otel = builder.Services.AddOpenTelemetry();
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

        builder.Services.AddTransient<ILogger>(p =>
        {
            return p.GetRequiredService<ILoggerFactory>().CreateLogger("API logger");
        });

        return builder;
    }
}