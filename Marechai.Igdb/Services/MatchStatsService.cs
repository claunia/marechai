using System;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Igdb.Services;

public class MatchStatsService
{
    readonly IDbContextFactory<MarechaiContext> _contextFactory;

    public MatchStatsService(IDbContextFactory<MarechaiContext> contextFactory) => _contextFactory = contextFactory;

    public async Task PrintAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        await PrintFor("Platforms", context.IgdbPlatforms.Select(p => p.MatchStatus));
        await PrintFor("Companies", context.IgdbCompanies.Select(c => c.MatchStatus));
        await PrintFor("Games", context.IgdbGames.Select(g => g.MatchStatus));

        int gameTypeCount = await context.IgdbGameTypes.CountAsync();
        Console.WriteLine($"\n  Game types mirrored: {gameTypeCount}");

        return;

        async Task PrintFor(string label, IQueryable<IgdbMatchStatus> statuses)
        {
            var counts = await statuses.GroupBy(s => s)
                                        .Select(g => new { Status = g.Key, Count = g.Count() })
                                        .ToListAsync();

            Console.WriteLine($"\n  {label}:");

            foreach(var c in counts.OrderBy(c => c.Status))
                Console.WriteLine($"    {c.Status,-12} {c.Count}");
        }
    }
}
