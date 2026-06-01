using Microsoft.EntityFrameworkCore;

public class MockDb : IDbContextFactory<BankDb>
{
    public BankDb CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<BankDb>()
            .UseInMemoryDatabase("bank-api-db")
            .Options;

        var context = new BankDb(options);
        context.Database.EnsureCreated();
        return context;
    }
}