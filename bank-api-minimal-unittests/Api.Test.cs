using Microsoft.AspNetCore.Http.HttpResults;

public class Test
{
    private static BankDb? databaseContext;

    [Before(Class)]
    public static Task CreateContext()
    {
        databaseContext = new MockDb().CreateDbContext();
        return Task.CompletedTask;
    }

    [Test]
    public async Task CreateBankReturnsCreated()
    {
        var result = await BankOperation.CreateBank(new BankModel()
        {
            BankTier = BankTier.A,
            Id = 123,
            IsCompliant = false
        }, databaseContext!);

        await Assert.That(result).IsTypeOf<Created<BankModel>>();
    }

    [Test, DependsOn(nameof(CreateBankReturnsCreated))]
    public async Task UpdateBankReturnsNoContent()
    {
        var result = await BankOperation.UpdateBank(123, new BankModel()
        {
            BankTier = BankTier.A,
            Id = 123,
            IsCompliant = true
        }, databaseContext!);

        await Assert.That(result).IsTypeOf<Results<NoContent, NotFound>>();
    }
}
