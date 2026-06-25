using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Igdb.Services;

public class GameMirrorService
{
    const int IgdbMaxPageSize = 500;

    readonly IDbContextFactory<MarechaiContext> _contextFactory;
    readonly IgdbHttpClient                     _client;

    public GameMirrorService(IDbContextFactory<MarechaiContext> contextFactory, IgdbHttpClient client)
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
        int  alreadyMirrored;

        await using(var probe = await _contextFactory.CreateDbContextAsync())
        {
            lastId          = await probe.IgdbGames.MaxAsync(g => (long?)g.IgdbId) ?? 0;
            alreadyMirrored = await probe.IgdbGames.CountAsync();
        }

        int total = 0;

        while(true)
        {
            int batchNumber = batchSize > 0 ? (alreadyMirrored + total) / batchSize + 1 : 1;

            (int count, long newLastId) = await FetchAndUpsertPageAsync(lastId, pageSize, batchNumber, dryRun);

            total += count;
            lastId = newLastId;

            if(!all || count < pageSize)
                break;
        }

        return total;
    }

    async Task<(int count, long lastId)> FetchAndUpsertPageAsync(long afterId, int pageSize, int batchNumber,
                                                                   bool dryRun)
    {
        string query = new ApicalypseQueryBuilder()
                      .Fields("id,name,game_type,parent_game,version_parent,platforms")
                      .Where($"id > {afterId}")
                      .Sort("id asc")
                      .Limit(pageSize)
                      .Build();

        using JsonDocument doc = await _client.QueryAsync("games", query);

        int  count  = doc.RootElement.GetArrayLength();
        long lastId = afterId;

        if(count == 0)
        {
            Console.WriteLine("  No more games to mirror.");

            return (0, lastId);
        }

        if(!dryRun)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            foreach(JsonElement element in doc.RootElement.EnumerateArray())
            {
                long   igdbId = element.GetProperty("id").GetInt64();
                string name   = element.GetProperty("name").GetString();

                int? gameTypeId = element.TryGetProperty("game_type", out var gt) && gt.ValueKind == JsonValueKind.Number
                                       ? gt.GetInt32()
                                       : null;

                long? parentGameId = element.TryGetProperty("parent_game", out var pg) &&
                                      pg.ValueKind == JsonValueKind.Number
                                          ? pg.GetInt64()
                                          : null;

                long? versionParentId = element.TryGetProperty("version_parent", out var vp) &&
                                         vp.ValueKind == JsonValueKind.Number
                                             ? vp.GetInt64()
                                             : null;

                List<int> platformIds = element.TryGetProperty("platforms", out var plats) &&
                                         plats.ValueKind == JsonValueKind.Array
                                             ? plats.EnumerateArray().Select(p => p.GetInt32()).ToList()
                                             : [];

                context.IgdbGames.Add(new IgdbGame
                {
                    IgdbId          = igdbId,
                    Name            = name,
                    GameTypeId      = gameTypeId,
                    ParentGameId    = parentGameId,
                    VersionParentId = versionParentId,
                    PlatformIdsJson = JsonSerializer.Serialize(platformIds),
                    MatchStatus     = IgdbMatchStatus.Pending,
                    BatchNumber     = batchNumber
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

        Console.WriteLine($"  Mirrored batch {batchNumber}: {count} games (up to IGDB id {lastId}).");

        return (count, lastId);
    }
}
