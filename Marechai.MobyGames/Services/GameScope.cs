using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.MobyGames.Services;

/// <summary>
///     Which already-imported games a per-game media pass should work on, shared by every
///     download/scrape command so they all accept the same options.
/// </summary>
/// <param name="Recent">
///     Consider only the N most recently imported games. Null means the whole catalogue.
/// </param>
/// <param name="Since">
///     Consider only games imported on or after this date. Null means no lower bound.
/// </param>
/// <param name="Force">
///     Re-visit games this pass has already been run over, instead of skipping them.
/// </param>
public readonly record struct GameScope(int? Recent = null, DateTime? Since = null, bool Force = false)
{
    public bool IsRecencyScoped => Recent is not null || Since is not null;

    /// <summary>
    ///     Parses the scoping options shared by every download/scrape command:
    ///     <c>--recent N</c>, <c>--since YYYY-MM-DD</c> and <c>--force</c>. Returns false and
    ///     prints the problem when a value is present but unusable.
    /// </summary>
    public static bool TryParse(string[] args, out GameScope scope)
    {
        int?      recent = null;
        DateTime? since  = null;

        for(int i = 1; i < args.Length; i++)
        {
            switch(args[i])
            {
                case "--recent" when i + 1 < args.Length:
                    if(!int.TryParse(args[i + 1], out int n) || n <= 0)
                    {
                        Console.WriteLine($"\e[31;1m--recent needs a positive number, got '{args[i + 1]}'\e[0m");
                        scope = default;

                        return false;
                    }

                    recent = n;

                    break;
                case "--since" when i + 1 < args.Length:
                    if(!DateTime.TryParse(args[i + 1], CultureInfo.InvariantCulture, DateTimeStyles.None,
                                          out DateTime d))
                    {
                        Console.WriteLine($"\e[31;1m--since needs a date (YYYY-MM-DD), got '{args[i + 1]}'\e[0m");
                        scope = default;

                        return false;
                    }

                    since = d;

                    break;
            }
        }

        scope = new GameScope(recent, since, args.Contains("--force"));

        return true;
    }

    /// <summary>Human-readable description of the scope, for the run header.</summary>
    public string Describe()
    {
        var parts = new List<string>();

        if(Recent is not null) parts.Add($"{Recent} most recent imports");
        if(Since is not null) parts.Add($"imported since {Since:yyyy-MM-dd}");
        if(Force) parts.Add("re-visiting already-processed games");

        return parts.Count == 0 ? "whole catalogue" : string.Join(", ", parts);
    }
}

/// <summary>
///     Selects the imported games a media pass should process. Every download/scrape service goes
///     through this so scoping, ordering and the already-visited skip behave identically in all of
///     them.
/// </summary>
public static class ImportedGameSelector
{
    /// <summary>
    ///     Returns at most <paramref name="batchSize" /> imported games for <paramref name="pass" />,
    ///     honouring <paramref name="scope" /> and skipping games the pass has already visited
    ///     (unless <see cref="GameScope.Force" />).
    /// </summary>
    /// <param name="requireExistingSoftware">
    ///     Also require the referenced Software row to still exist: stale states (Software deleted
    ///     after import) would otherwise cause an FK violation on insert.
    /// </param>
    /// <param name="usePassState">
    ///     False when the caller already has its own per-game resume marker (the review import has
    ///     one in <c>MobyGamesReviewImportState</c>) and applies it itself; a second marker for the
    ///     same work would only drift out of sync.
    /// </param>
    public static async Task<List<MobyGamesImportState>> SelectAsync(
        MarechaiContext context, MediaPassStateService passState, MobyGamesMediaPass pass, GameScope scope,
        int batchSize, bool requireExistingSoftware = false, bool usePassState = true)
    {
        IQueryable<MobyGamesImportState> query = context.MobyGamesImportStates
                                                        .Where(s => s.Status == MobyGamesImportStatus.Imported &&
                                                                    s.SoftwareId != null);

        if(requireExistingSoftware)
            query = query.Where(s => context.Softwares.Any(sw => sw.Id == s.SoftwareId.Value));

        // Recency needs an import timestamp. Imports have stamped ProcessedOn for a long time, but
        // a state row predating that (or written by hand) has none, and such a game can never be
        // reached by --since and sorts last under --recent. Say so rather than silently omitting it:
        // an unscoped run still covers the whole catalogue, timestamps or not.
        if(scope.IsRecencyScoped)
        {
            int undated = await query.CountAsync(s => s.ProcessedOn == null);

            if(undated > 0)
            {
                Console.WriteLine($"  \e[33;1m{undated} imported game(s) have no import date\e[0m: " +
                                  (scope.Since is not null
                                       ? "--since cannot see them."
                                       : "--recent sorts them last, by insertion order.") +
                                  " Run without --recent/--since to cover them.");
            }
        }

        if(scope.Since is not null)
            query = query.Where(s => s.ProcessedOn != null && s.ProcessedOn >= scope.Since.Value);

        // Recency-scoped runs want the newest imports first, falling back to insertion order so a
        // run is still deterministic when timestamps are missing; an unscoped sweep keeps the
        // historical slug ordering so its progress through the catalogue stays predictable.
        query = scope.IsRecencyScoped
                    ? query.OrderByDescending(s => s.ProcessedOn).ThenByDescending(s => s.Id)
                    : query.OrderBy(s => s.MobyGameId);

        if(scope.Recent is not null) query = query.Take(scope.Recent.Value);

        List<MobyGamesImportState> candidates = await query.ToListAsync();

        // The caller filters and takes its own batch after applying its own resume marker.
        if(!usePassState) return candidates;

        if(scope.Force) return candidates.Take(batchSize).ToList();

        HashSet<string> visited = await passState.GetVisitedGameIdsAsync(pass);

        return candidates.Where(g => !visited.Contains(g.MobyGameId)).Take(batchSize).ToList();
    }
}
