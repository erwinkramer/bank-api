
static partial class ApiMapper
{
    public static WebApplication MapTellerEndpoints(this WebApplication app)
    {
        var tellerItems = app.MapGroup("/teller")
            .WithTags("Administrative")
            .RequireRateLimiting("fixed")
            .RequireAuthorization("bank_god");

        tellerItems.MapGet("/", TellerOperation.GetBankTeller)
            .WithName("GetBankTeller").WithSummary("Get bank teller")
            .WithDescription("Get the teller of all banks.");

        return app;
    }
}