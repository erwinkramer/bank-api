using Microsoft.EntityFrameworkCore;

public class MockDb : IDbContextFactory<BankDb>
{
    public BankDb CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<BankDb>()
            .UseInMemoryDatabase("bank-api-db");

        BankDb.ConfigureOptions(optionsBuilder);

        var options = optionsBuilder.Options;

        var context = new BankDb(options);
        context.Database.EnsureCreated();
        return context;
    }
}