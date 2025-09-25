using System.Text.Json.Serialization;

public static partial class ApiBuilder
{
    public static IServiceCollection ConfigureJson(this IServiceCollection services)
    {
        services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
        {
            options.SerializerOptions.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow;
            options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict;
        });
        return services;
    }
}