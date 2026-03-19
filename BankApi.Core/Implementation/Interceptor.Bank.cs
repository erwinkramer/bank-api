using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

public class BankEventInterceptor : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var dbContext = eventData.Context;

        if (dbContext is not null)
        {
            await AddToOutbox(dbContext);
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private async static Task AddToOutbox(DbContext dbContext)
    {
        var bankEntries = dbContext.ChangeTracker.Entries<BankModel>().ToList();

        foreach (var entry in bankEntries)
        {
            if (entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
            {
                continue; // skip unchanged or detached entities
            }

            var outboxEntry = new BankEventOutboxModel
            {
                BankId = entry.Entity.Id,
                EventSubtype = entry.State.ToString().ToLower()
            };

            await dbContext.AddAsync(outboxEntry);
        }
    }
}