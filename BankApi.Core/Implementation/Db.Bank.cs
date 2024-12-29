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
                    new ()
                    {
                        Id = Guid.Parse("29e26195-cf57-417d-ac1b-998398e2dc88"),
                        BankTier = BankTier.A,
                        IsCompliant = true,
                        Name = "Foo"
                    },
                    new ()
                    {
                        Id = Guid.Parse("bed8a856-1d6a-4c2e-9392-a126a7eda415"),
                        BankTier = BankTier.B,
                        IsCompliant = false,
                        Name = "Bar"
                    }
                ];

                context.Set<BankModel>().AddRange(data);
                context.SaveChanges();
            });
}