using Microsoft.EntityFrameworkCore;

public class MockDb : IDbContextFactory<BankDb>
{
    public BankDb CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<BankDb>()
            .UseInMemoryDatabase(GlobalConfiguration.ApiSettings!.DatabaseName)
            .Options;

        return new (options);
    }
}