using CloudNative.CloudEvents;
using CloudNative.CloudEvents.Http;
using CloudNative.CloudEvents.SystemTextJson;
using Gridify.EntityFramework;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Caching.Hybrid;

public class BankOperation
{
    public static async Task<Results<Ok<Paging<BankModel>>, UnprocessableEntity>> GetAllBanks([AsParameters] GridQuery query, BankDb db, HybridCache cache, CancellationToken token = default)
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

    public static async Task<Results<Ok<BankModel>, NotFound, UnprocessableEntity>> GetBank([Bank] Guid id, BankDb db, HybridCache cache, CancellationToken token = default)
    {
        return await cache.GetOrCreateAsync(
        $"bank-{id}",
        async innerToken => await db.Banks.FindAsync(id, innerToken), cancellationToken: token)
            is BankModel bank
                ? TypedResults.Ok(bank)
                : TypedResults.NotFound();
    }

    public static async Task<Results<Created<BankModel>, UnprocessableEntity>> CreateBank(BankModel bank, BankDb db, HybridCache? cache)
    {
        await db.Banks.AddAsync(bank);
        await db.SaveChangesAsync();
        if (cache != null)
            await cache.RemoveByTagAsync("banks");

        await CreateBankEvent(bank);
        return TypedResults.Created($"/bankitems/{bank.Id}", bank);
    }

    /// <summary>
    /// Typically events would be stored in an outbox table 
    /// and a background service would be responsible for dispatching them, 
    /// including retry logic and dead letter handling. 
    /// For simplicity, this example directly sends the event after the bank is created.
    /// </summary>
    /// <param name="bank">The bank model that was created.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private static async Task CreateBankEvent(BankModel bank)
    {
        var bankEvent = new BankEvent("created")
        {
            Data = new
            {
                bankId = bank.Id
            }
        };

        using var httpClient = new HttpClient();
        var content = bankEvent.CloudEvent.ToHttpContent(ContentMode.Structured, new JsonEventFormatter());
        var result = await httpClient.PostAsync("https://webhook.site/cf839315-925e-4af1-b903-1a09da5a0d70", content);
    }

    public static async Task<Results<NoContent, NotFound, UnprocessableEntity>> UpdateBank([Bank] Guid id, BankModel inputBank, BankDb db, HybridCache? cache)
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

    public static async Task<Results<NoContent, NotFound, UnprocessableEntity>> DeleteBank([Bank] Guid id, BankDb db, HybridCache cache)
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