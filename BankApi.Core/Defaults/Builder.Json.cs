using System.Text.Json;
using System.Text.Json.Serialization;
using CloudNative.CloudEvents.SystemTextJson;

public static partial class ApiBuilder
{
    public static IServiceCollection ConfigureJson(this IServiceCollection services)
    {
        services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
        {
            options.SerializerOptions.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow;
            options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict;
        });

        var jsonEventFormatter = new JsonEventFormatter(
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters =
                {
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                }
            },
            new JsonDocumentOptions());

        GlobalConfiguration.JsonEventFormatter = jsonEventFormatter;

        return services;
    }
}