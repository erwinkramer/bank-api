using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Configuration;

namespace BankApi.Tests.Operations;

public class BankOperationTests
{
    private static BankDb? databaseContext;
    private static string bankId = "1cfd33e8-0ec0-45f5-8ed9-53750d1e1d81";

    [Before(Class)]
    public static Task CreateContext()
    {
        var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        GlobalConfiguration.ApiSettings = config.GetRequiredSection("ApiSettings").Get<GlobalConfiguration.SettingsModel>()!;

        databaseContext = new MockDb().CreateDbContext();
        return Task.CompletedTask;
    }

    [Test]
    public async Task CreateBankReturnsCreated()
    {
        var httpContext = new DefaultHttpContext();
        var response = await BankOperation.CreateBank(new()
        {
            BankTier = BankTier.A,
            Id = Guid.Parse(bankId),
            IsCompliant = false
        }, httpContext, databaseContext!, null);

        await Assert.That(response.Result).IsTypeOf<Created<BankModel>>();
        await Assert.That(httpContext.Response.Headers.ETag).IsNotNull().And.IsNotEmpty();
        TestContext.Current!.StateBag["ETag"] = httpContext.Response.Headers.ETag;
    }

    [Test, DependsOn(nameof(CreateBankReturnsCreated))]
    public async Task UpdateBankReturnsNoContent()
    {
        var httpContext = new DefaultHttpContext();
        var createBankContext = TestContext.Current!.Dependencies.GetTests(nameof(CreateBankReturnsCreated)).First();
        httpContext.Request.Headers.IfMatch = createBankContext.StateBag.Items["ETag"]?.ToString();

        var response = await BankOperation.UpdateBank(Guid.Parse(bankId), new()
        {
            BankTier = BankTier.A,
            Id = Guid.Parse(bankId),
            IsCompliant = true
        }, httpContext, databaseContext!, null);

        await Assert.That(response.Result).IsTypeOf<NoContent>();
    }
}
