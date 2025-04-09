public class AddApiVersionHeaderMiddleware
{
    private readonly RequestDelegate _next;

    public AddApiVersionHeaderMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Adds the API version header to the response. This is useful for clients to know which version of the API they are using. And to comply to NLGov REST API Design Rules.
    /// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/best-practices?view=aspnetcore-9.0#do-not-modify-the-status-code-or-headers-after-the-response-body-has-started
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            context.Response.Headers["API-Version"] = GlobalConfiguration.ApiDocument!.Info.Version;
            return Task.CompletedTask;
        });

        await _next(context);
    }
}