using System;
using System.Text.Json;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Igdb.Services;

public class PlatformMirrorService
{
    const int IgdbMaxPageSize = 500;

    readonly IDbContextFactory<MarechaiContext> _contextFactory;
    readonly IgdbHttpClient                     _client;

    public PlatformMirrorService(IDbContextFactory<MarechaiContext> contextFactory, IgdbHttpClient client)
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

        int offset;

        await using(var probe = await _contextFactory.CreateDbContextAsync())
            offset = await probe.IgdbPlatforms.CountAsync();

        int total = 0;

        while(true)
        {
            int count = await FetchAndUpsertPageAsync(offset + total, pageSize, dryRun);

            total += count;

            if(!all || count < pageSize)
                break;
        }

        return total;
    }

    async Task<int> FetchAndUpsertPageAsync(int offset, int pageSize, bool dryRun)
    {
        string query = new ApicalypseQueryBuilder().Fields("id,name")
                                                     .Sort("id asc")
                                                     .Limit(pageSize)
                                                     .Offset(offset)
                                                     .Build();

        using JsonDocument doc = await _client.QueryAsync("platforms", query);

        int count = doc.RootElement.GetArrayLength();

        if(count == 0)
        {
            Console.WriteLine("  No more platforms to mirror.");

            return 0;
        }

        if(!dryRun)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            foreach(JsonElement element in doc.RootElement.EnumerateArray())
            {
                int    igdbId = element.GetProperty("id").GetInt32();
                string name   = element.GetProperty("name").GetString();

                var existing = await context.IgdbPlatforms.FirstOrDefaultAsync(p => p.Id == igdbId);

                if(existing == null)
                {
                    context.IgdbPlatforms.Add(new IgdbPlatform
                    {
                        Id          = igdbId,
                        Name        = name,
                        MatchStatus = IgdbMatchStatus.Pending
                    });
                }
                else
                {
                    existing.Name = name;
                }
            }

            await context.SaveChangesAsync();
        }

        Console.WriteLine($"  Mirrored {count} platforms (offset {offset}).");

        return count;
    }
}
