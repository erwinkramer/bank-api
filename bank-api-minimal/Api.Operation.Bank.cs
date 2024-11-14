using Gridify;
using Gridify.EntityFramework;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Caching.Hybrid;
using System.ComponentModel;

public class BankOperation
{
    public static async Task<Ok<Paging<BankModel>>> GetAllBanks([AsParameters] GridQuery query, BankDb db)
    {
        return TypedResults.Ok(await db.Banks.GridifyAsync(query));;
    }

    public static async Task<Results<Ok<BankModel>, NotFound>> GetBank([DefaultValue(1)] int id, BankDb db, HybridCache cache, CancellationToken token = default)
    {
        return await cache.GetOrCreateAsync(
           $"bank-{id}",
           async cancel => await db.Banks.FindAsync(id, token), cancellationToken: token)
            is BankModel bank
                ? TypedResults.Ok(bank)
                : TypedResults.NotFound();
    }

    public static async Task<Created<BankModel>> CreateBank(BankModel bank, BankDb db)
    {
        db.Banks.Add(bank);
        await db.SaveChangesAsync();

        return TypedResults.Created($"/bankitems/{bank.Id}", bank);
    }

    public static async Task<Results<NoContent, NotFound>> UpdateBank(int id, BankModel inputBank, BankDb db)
    {
        var bank = await db.Banks.FindAsync(id);

        if (bank is null) return TypedResults.NotFound();

        bank.Name = inputBank.Name;
        bank.IsCompliant = inputBank.IsCompliant;

        await db.SaveChangesAsync();

        return TypedResults.NoContent();
    }

    public static async Task<Results<NoContent, NotFound>> DeleteBank(int id, BankDb db)
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