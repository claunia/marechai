using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.MobyGames.Services;

/// <summary>
///     Tracks which games each per-game media pass has already visited, so a pass advances through
///     the catalogue across runs instead of re-checking the same games forever.
/// </summary>
public class MediaPassStateService
{
    readonly IDbContextFactory<MarechaiContext> _contextFactory;

    public MediaPassStateService(IDbContextFactory<MarechaiContext> contextFactory) =>
        _contextFactory = contextFactory;

    /// <summary>Slugs this pass has already visited.</summary>
    public async Task<HashSet<string>> GetVisitedGameIdsAsync(MobyGamesMediaPass pass)
    {
        await using MarechaiContext context = await _contextFactory.CreateDbContextAsync();

        List<string> ids = await context.MobyGamesMediaPassStates
                                        .Where(s => s.Pass == pass)
                                        .Select(s => s.MobyGameId)
                                        .ToListAsync();

        return new HashSet<string>(ids, StringComparer.Ordinal);
    }

    /// <summary>
    ///     Marks games as visited by <paramref name="pass" />. Already-recorded games are left
    ///     alone, so this is safe to call repeatedly (and after a <c>--force</c> re-run).
    /// </summary>
    public async Task MarkVisitedAsync(MobyGamesMediaPass pass, IReadOnlyCollection<string> gameIds)
    {
        if(gameIds.Count == 0) return;

        await using MarechaiContext context = await _contextFactory.CreateDbContextAsync();

        HashSet<string> already = (await context.MobyGamesMediaPassStates
                                                .Where(s => s.Pass == pass && gameIds.Contains(s.MobyGameId))
                                                .Select(s => s.MobyGameId)
                                                .ToListAsync()).ToHashSet(StringComparer.Ordinal);

        DateTime now = DateTime.UtcNow;

        foreach(string gameId in gameIds.Distinct(StringComparer.Ordinal).Where(id => !already.Contains(id)))
        {
            context.MobyGamesMediaPassStates.Add(new MobyGamesMediaPassState
            {
                MobyGameId  = gameId,
                Pass        = pass,
                ProcessedOn = now
            });
        }

        await context.SaveChangesAsync();
    }

    /// <summary>
    ///     Forgets that <paramref name="pass" /> visited <paramref name="gameId" />, so the next run
    ///     of that pass processes it again.
    ///     <para>
    ///         Used by the scrapers: a download pass marks a game visited even when its source page
    ///         was not cached yet (otherwise it would re-check the same pageless games forever and
    ///         never advance through the catalogue). When the scraper later fetches that page, the
    ///         downstream pass has new work to do, so its marker has to go.
    ///     </para>
    /// </summary>
    public async Task ClearVisitedAsync(MobyGamesMediaPass pass, string gameId)
    {
        await using MarechaiContext context = await _contextFactory.CreateDbContextAsync();

        MobyGamesMediaPassState row = await context.MobyGamesMediaPassStates
                                                  .FirstOrDefaultAsync(s => s.Pass == pass &&
                                                                            s.MobyGameId == gameId);

        if(row is null) return;

        context.MobyGamesMediaPassStates.Remove(row);
        await context.SaveChangesAsync();
    }
}
