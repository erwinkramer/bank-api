using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

public static partial class ApiBuilder
{
    public static IHostApplicationBuilder AddLoggingServices(this IHostApplicationBuilder builder)
    {
        var otel = builder.Services.AddOpenTelemetry();
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