using Microsoft.EntityFrameworkCore;

public class BankDb : DbContext
{
    public BankDb(DbContextOptions<BankDb> options)
            : base(options) { }

    public DbSet<BankModel> Banks => Set<BankModel>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder
            .UseSeeding((context, _) =>
            {
                BankModel[] data = [
                    new BankModel
                    {
                        Id = 1,
                        BankTier = BankTier.A,
                        IsCompliant = true,
                        Name = "Foo"
                    },
                    new BankModel
                    {
                        Id = 2,
                        BankTier = BankTier.B,
                        IsCompliant = false,
                        Name = "Bar"
                    }
                ];

                context.Set<BankModel>().AddRange(data);
                context.SaveChanges();
            });
}