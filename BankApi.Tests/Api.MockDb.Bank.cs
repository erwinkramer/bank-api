using Microsoft.EntityFrameworkCore;

public class MockDb : IDbContextFactory<BankDb>
{
    public BankDb CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<BankDb>()
            .UseInMemoryDatabase($"InMemoryTestDb-{DateTime.Now.ToFileTimeUtc()}")
            .Options;

        return new BankDb(options);
    }
}