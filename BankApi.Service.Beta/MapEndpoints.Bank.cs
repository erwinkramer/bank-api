public static partial class ApiMapper
{
    public static WebApplication MapBankEndpoints(this WebApplication app)
    {
        var bankItems = app.MapGroup("/banks")
            .WithTags("Supervisory")
            .RequireRateLimiting("fixed")
            .RequireAuthorization("bank_subscription")
            .RequireCors("generic");

        bankItems.MapGet("/", BankOperation.GetAllBanks)
            .WithName("GetAllBanks").WithSummary("Get all banks")
            .WithDescription("Get all banks in the Netherlands.");

        bankItems.MapGet("/{id}", BankOperation.GetBank)
            .WithName("GetBank").WithSummary("Get a bank")
            .WithDescription("Get a bank in the Netherlands.");

        bankItems.MapPost("/", BankOperation.CreateBank)
            .WithName("CreateBank").WithSummary("Create a bank")
            .WithDescription("Create a bank in the Netherlands.");

        bankItems.MapPut("/{id}", BankOperation.UpdateBank)
            .WithName("UpdateBank").WithSummary("Update a bank")
            .WithDescription("Update a bank in the Netherlands.");

        bankItems.MapDelete("/{id}", BankOperation.DeleteBank)
            .WithName("DeleteBank").WithSummary("Delete a bank")
            .WithDescription("Delete a bank in the Netherlands.");

        return app;
    }
}