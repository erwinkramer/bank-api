using Gridify.EntityFramework;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Hybrid;

public class BankOperation
{
    public static async Task<Results<Ok<Paging<BankModel>>, UnprocessableEntity>> GetAllBanks([AsParameters] GridQuery query, [FromServices] BankDb db, HybridCache cache, CancellationToken token = default)
    {
        var cachedResult = await cache.GetOrCreateAsync(
            $"banks-{query.Page}-{query.PageSize}-{query.Filter}-{query.OrderBy}",
            async innerToken =>
            {
                var pagingResult = await db.Banks.GridifyAsync(query, innerToken);
                return new Paging<BankModel>(pagingResult.Count, pagingResult.Data);
            },
            cancellationToken: token,
            tags: ["banks"]
            );

        return TypedResults.Ok(cachedResult);
    }

    public static async Task<Results<Ok<BankModel>, NotFound, UnprocessableEntity>> GetBank([Bank] Guid id, [FromServices] BankDb db, HybridCache cache, CancellationToken token = default)
    {
        return await cache.GetOrCreateAsync(
        $"bank-{id}",
        async innerToken => await db.Banks.FindAsync(id, innerToken), cancellationToken: token)
            is BankModel bank
                ? TypedResults.Ok(bank)
                : TypedResults.NotFound();
    }

    public static async Task<Results<Created<BankModel>, UnprocessableEntity>> CreateBank(BankModel bank, [FromServices] BankDb db, HybridCache? cache)
    {
        await db.Banks.AddAsync(bank);
        await db.SaveChangesAsync();
        if (cache != null)
            await cache.RemoveByTagAsync("banks");
        return TypedResults.Created($"/bankitems/{bank.Id}", bank);
    }

    public static async Task<Results<NoContent, NotFound, UnprocessableEntity>> UpdateBank([Bank] Guid id, BankModel inputBank, [FromServices] BankDb db, HybridCache? cache)
    {
        var bank = await db.Banks.FindAsync(id);
        if (bank is null) return TypedResults.NotFound();

        bank.Name = inputBank.Name;
        bank.IsCompliant = inputBank.IsCompliant;

        await db.SaveChangesAsync();
        if (cache != null)
        {
            await cache.RemoveAsync($"bank-{id}");
            await cache.RemoveByTagAsync("banks");
        }

        return TypedResults.NoContent();
    }

    public static async Task<Results<NoContent, NotFound, UnprocessableEntity>> DeleteBank([Bank] Guid id, [FromServices] BankDb db, HybridCache cache)
    {
        if (await db.Banks.FindAsync(id) is BankModel bank)
        {
            db.Banks.Remove(bank);
            await db.SaveChangesAsync();
            await cache.RemoveAsync($"bank-{id}");
            await cache.RemoveByTagAsync("banks");

            return TypedResults.NoContent();
        }

        return TypedResults.NotFound();
    }
}