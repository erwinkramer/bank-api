using System.Data.Common;
using Microsoft.AspNetCore.Diagnostics;

public static partial class ApiBuilder
{
    public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(exceptionHandlerApp => exceptionHandlerApp.Run(async context =>
        {
            var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
            switch (exceptionFeature?.Error)
            {
                case InvalidOperationException or ArgumentException:
                    await Results.UnprocessableEntity().ExecuteAsync(context);
                    return;
                case BadHttpRequestException or FormatException:
                    await Results.BadRequest().ExecuteAsync(context);
                    return;
            }

            await Results.Problem().ExecuteAsync(context);
        }));

        return app;
    }
}
