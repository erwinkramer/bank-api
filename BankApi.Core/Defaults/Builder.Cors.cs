public static partial class ApiBuilder
{
    public static IServiceCollection AddCorsServices(this IServiceCollection services)
    {
        services.AddCors(options =>
         {
             options.AddPolicy("generic", policy =>
             {
                 policy.WithOrigins("*")
                       .AllowAnyHeader()
                       .AllowAnyMethod()
                       .AllowAnyOrigin()
                       .WithExposedHeaders("Access-Control-Allow-Origin");
             });
         });

        return services;
    }
}