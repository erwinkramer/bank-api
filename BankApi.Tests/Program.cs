using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Configuration;

public class Test
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
        var response = await BankOperation.CreateBank(new()
        {
            BankTier = BankTier.A,
            Id = Guid.Parse(bankId),
            IsCompliant = false
        }, databaseContext!, null);

        await Assert.That(response.Result).IsTypeOf<Created<BankModel>>();
    }

    [Test, DependsOn(nameof(CreateBankReturnsCreated))]
    public async Task UpdateBankReturnsNoContent()
    {
        var response = await BankOperation.UpdateBank(Guid.Parse(bankId), new()
        {
            BankTier = BankTier.A,
            Id = Guid.Parse(bankId),
            IsCompliant = true
        }, databaseContext!, null);

        await Assert.That(response.Result).IsTypeOf<NoContent>();
    }
}
