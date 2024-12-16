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
            Id = Guid.NewGuid(),
            IsCompliant = false
        }, databaseContext!);

        await Assert.That(result).IsTypeOf<Created<BankModel>>();
    }

    [Test, DependsOn(nameof(CreateBankReturnsCreated))]
    public async Task UpdateBankReturnsNoContent()
    {
        var bankId = Guid.NewGuid();
        var result = await BankOperation.UpdateBank(bankId, new BankModel()
        {
            BankTier = BankTier.A,
            Id = bankId,
            IsCompliant = true
        }, databaseContext!);

        await Assert.That(result).IsTypeOf<Results<NoContent, NotFound>>();
    }
}
