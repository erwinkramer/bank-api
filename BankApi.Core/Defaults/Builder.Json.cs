using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using CloudNative.CloudEvents.SystemTextJson;

public static partial class ApiBuilder
{
    public static IServiceCollection ConfigureJson(this IServiceCollection services)
    {
        services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
        {
            options.SerializerOptions.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow;
            options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict;
            var jsonResolver = new DefaultJsonTypeInfoResolver();
            jsonResolver.Modifiers.Add(static typeInfo =>
            {
                var property = typeInfo.Properties.FirstOrDefault(p =>
                    p.AttributeProvider?.IsDefined(typeof(ConcurrencyCheckAttribute), inherit: true) == true);

                if (property is not null)
                {
                    property.Set = null;
                    property.ShouldSerialize = static (_, _) => false;
                }
            });
            options.SerializerOptions.TypeInfoResolverChain.Insert(0, jsonResolver);
        });

        GlobalConfiguration.JsonEventFormatter = new JsonEventFormatter(
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters =
                {
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                }
            },
            new JsonDocumentOptions());

        return services;
    }
}