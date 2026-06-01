using Microsoft.EntityFrameworkCore;

public class BankDb : DbContext
{
    public BankDb(DbContextOptions<BankDb> options)
            : base(options) { }

    public DbSet<BankModel> Banks => Set<BankModel>();

    public DbSet<BankEventOutboxModel> Outbox => Set<BankEventOutboxModel>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder
            .AddInterceptors(new BankEventInterceptor())
            .UseSeeding((context, _) =>
            {
                BankModel[] data = [
                    new ()
                    {
                        Id = Guid.NewGuid(),
                        BankTier = BankTier.A,
                        IsCompliant = true,
                        Name = $"Bank branch {Random.Shared.Next(10, 1000)}"
                    },
                    new ()
                    {
                        Id = Guid.NewGuid(),
                        BankTier = BankTier.B,
                        IsCompliant = false,
                        Name = $"Bank branch {Random.Shared.Next(10, 1000)}"
                    }
                ];

                context.Set<BankModel>().AddRange(data);
                context.SaveChanges();
            });
}