using Gridify.EntityFramework;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Caching.Hybrid;

public class BankOperation
{
    public static async Task<Results<Ok<Paging<BankModel>>, UnprocessableEntity>> GetAllBanks([AsParameters] GridQuery query, BankDb db)
    {
        var pagingResult = await db.Banks.GridifyAsync(query);
        return TypedResults.Ok(new Paging<BankModel>(pagingResult.Count, pagingResult.Data));
    }

    public static async Task<Results<Ok<BankModel>, NotFound, UnprocessableEntity>> GetBank([Bank][Id] Guid id, BankDb db, HybridCache cache, CancellationToken token = default)
    {
        return await cache.GetOrCreateAsync(
        $"bank-{id}",
        async cancel => await db.Banks.FindAsync(id, token), cancellationToken: token)
            is BankModel bank
                ? TypedResults.Ok(bank)
                : TypedResults.NotFound();
    }

    public static async Task<Results<Created<BankModel>, UnprocessableEntity>> CreateBank(BankModel bank, BankDb db)
    {
        db.Banks.Add(bank);
        await db.SaveChangesAsync();
        return TypedResults.Created($"/bankitems/{bank.Id}", bank);
    }

    public static async Task<Results<NoContent, NotFound, UnprocessableEntity>> UpdateBank([Bank][Id] Guid id, BankModel inputBank, BankDb db)
    {
        var bank = await db.Banks.FindAsync(id);
        if (bank is null) return TypedResults.NotFound();

        bank.Name = inputBank.Name;
        bank.IsCompliant = inputBank.IsCompliant;

        await db.SaveChangesAsync();

        return TypedResults.NoContent();
    }

    public static async Task<Results<NoContent, NotFound, UnprocessableEntity>> DeleteBank([Bank][Id] Guid id, BankDb db)
    {
        if (await db.Banks.FindAsync(id) is BankModel bank)
        {
            db.Banks.Remove(bank);
            await db.SaveChangesAsync();
            return TypedResults.NoContent();
        }

        return TypedResults.NotFound();
    }
}