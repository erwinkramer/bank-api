using System.Text.Json;
public static partial class ApiBuilder
{
    public static IServiceCollection AddDapr(this IServiceCollection services)
    {
        services.AddDaprClient(daprBuilder =>
        {
            daprBuilder.UseJsonSerializationOptions(new JsonSerializerOptions
            {
                WriteIndented = true,
                MaxDepth = 8
            });
            daprBuilder.UseTimeout(TimeSpan.FromSeconds(30));
        });

        return services;
    }
}