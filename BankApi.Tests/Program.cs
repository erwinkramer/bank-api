using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Configuration;

public class Test
{
    private static BankDb? databaseContext;
    private static string bankId = "29e26195-cf57-417d-ac1b-998398e2dc88";

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
        }, databaseContext!);

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
        }, databaseContext!);

        await Assert.That(response.Result).IsTypeOf<NoContent>();
    }
}
