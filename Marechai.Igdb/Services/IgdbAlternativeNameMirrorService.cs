using System;
using System.Text.Json;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Igdb.Services;

public class IgdbAlternativeNameMirrorService
{
    const int IgdbMaxPageSize = 500;

    readonly IDbContextFactory<MarechaiContext> _contextFactory;
    readonly IgdbHttpClient                     _client;

    public IgdbAlternativeNameMirrorService(IDbContextFactory<MarechaiContext> contextFactory, IgdbHttpClient client)
    {
        _contextFactory = contextFactory;
        _client         = client;
    }

    /// <summary>Mirrors one batch (one IGDB request, at most <paramref name="batchSize" /> rows, clamped to
    /// IGDB's 500-row page limit). Run the command again to mirror the next batch. Pass <paramref name="batchSize" />
    /// = 0 to instead loop until the whole catalog has been mirrored in this single invocation.</summary>
    public async Task<int> RunAsync(int batchSize, bool dryRun)
    {
        bool all      = batchSize <= 0;
        int  pageSize = all ? IgdbMaxPageSize : Math.Min(batchSize, IgdbMaxPageSize);

        long lastId;

        await using(var probe = await _contextFactory.CreateDbContextAsync())
            lastId = await probe.IgdbAlternativeNames.MaxAsync(a => (long?)a.IgdbId) ?? 0;

        int total = 0;

        while(true)
        {
            (int count, long newLastId) = await FetchAndUpsertPageAsync(lastId, pageSize, dryRun);

            total += count;
            lastId = newLastId;

            if(!all || count < pageSize)
                break;
        }

        return total;
    }

    async Task<(int count, long lastId)> FetchAndUpsertPageAsync(long afterId, int pageSize, bool dryRun)
    {
        string query = new ApicalypseQueryBuilder().Fields("id,game,name,comment")
                                                     .Where($"id > {afterId}")
                                                     .Sort("id asc")
                                                     .Limit(pageSize)
                                                     .Build();

        using JsonDocument doc = await _client.QueryAsync("alternative_names", query);

        int  count  = doc.RootElement.GetArrayLength();
        long lastId = afterId;

        if(count == 0)
        {
            Console.WriteLine("  No more alternative names to mirror.");

            return (0, lastId);
        }

        if(!dryRun)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            foreach(JsonElement element in doc.RootElement.EnumerateArray())
            {
                long igdbId = element.GetProperty("id").GetInt64();

                // Some alternative_names rows are not attached to a game (e.g. attached to a
                // collection/franchise instead) — skip those, we only care about game titles.
                if(!element.TryGetProperty("game", out JsonElement gameProp) ||
                   gameProp.ValueKind != JsonValueKind.Number)
                {
                    lastId = igdbId;

                    continue;
                }

                context.IgdbAlternativeNames.Add(new IgdbAlternativeName
                {
                    IgdbId     = igdbId,
                    GameIgdbId = gameProp.GetInt64(),
                    Name       = element.GetProperty("name").GetString(),
                    Comment = element.TryGetProperty("comment", out JsonElement c) && c.ValueKind == JsonValueKind.String
                                  ? c.GetString()
                                  : null
                });

                lastId = igdbId;
            }

            await context.SaveChangesAsync();
        }
        else
        {
            foreach(JsonElement element in doc.RootElement.EnumerateArray())
                lastId = element.GetProperty("id").GetInt64();
        }

        Console.WriteLine($"  Mirrored {count} alternative names (up to IGDB id {lastId}).");

        return (count, lastId);
    }
}
