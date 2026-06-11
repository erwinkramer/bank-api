using System.Text.Json;
using Dapr.SecretsManagement.Extensions;

public static partial class ApiBuilder
{
    public static IServiceCollection AddDapr(this IServiceCollection services)
    {
        services.AddDaprSecretsManagementClient((_, daprBuilder) =>
        {
            daprBuilder.UseJsonSerializationOptions(new JsonSerializerOptions
            {
                WriteIndented = true,
                MaxDepth = 8
            });
            daprBuilder.UseGrpcEndpoint("http://localhost:50002"); //The gRPC endpoint must use http or https
            daprBuilder.UseTimeout(TimeSpan.FromSeconds(30));
        });

        return services;
    }
}