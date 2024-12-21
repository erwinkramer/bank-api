public static partial class ApiBuilder
{
    public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(exceptionHandlerApp
            => exceptionHandlerApp.Run(async context
                => await Results.Problem().ExecuteAsync(context)));

        return app;
    }
}
