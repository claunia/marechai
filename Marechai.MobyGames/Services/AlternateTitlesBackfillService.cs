using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Database.Models;
using Marechai.MobyGames.Models;
using Marechai.MobyGames.Parsers;
using Microsoft.EntityFrameworkCore;

namespace Marechai.MobyGames.Services;

/// <summary>
///     Backfills <see cref="SoftwareAlternativeTitle" /> rows for already-imported games from the
///     cached MobyGames HTML in <c>mobygames_raw</c>. The importer ignored the "Alternate Titles"
///     section until now, so the data is sitting unused in the cache for roughly one game in six.
///     Purely cache-driven: no network access.
///     <para>
///         Additive only — see <see cref="AlternateTitleWriter" /> for the exact insert /
///         fill-blank-comment / never-delete semantics, which this shares with the importer.
///     </para>
/// </summary>
public class AlternateTitlesBackfillService
{
    readonly IDbContextFactory<MarechaiContext> _contextFactory;
    readonly SourceDatabaseService              _sourceDb;

    public AlternateTitlesBackfillService(IDbContextFactory<MarechaiContext> contextFactory,
                                         SourceDatabaseService sourceDb)
    {
        _contextFactory = contextFactory;
        _sourceDb       = sourceDb;
    }

    /// <summary>
    ///     Backfills a single game, identified by its MobyGames slug. Lists every alternate title
    ///     found in the cache before touching the database, so the parser can be verified against
    ///     a known page even when the game is not imported locally.
    /// </summary>
    public async Task RunForSlugAsync(string slug, bool dryRun)
    {
        Console.WriteLine($"Backfilling alternate titles for '{slug}' (dryRun={dryRun})\n");

        List<ParsedAlternateTitle> found = await ParseAlternateTitlesForSlugAsync(slug);

        Console.WriteLine($"  {found.Count} alternate title(s) in cached HTML:");

        foreach(ParsedAlternateTitle title in found)
            Console.WriteLine($"    \"{title.Title}\"{(title.Comment is null ? "" : $" -- {title.Comment}")}");

        Console.WriteLine();

        await using MarechaiContext context = await _contextFactory.CreateDbContextAsync();

        MobyGamesImportState state = await context.MobyGamesImportStates
                                                 .FirstOrDefaultAsync(s => s.MobyGameId == slug);

        if(state is null)
        {
            Console.WriteLine($"No import state row for '{slug}': nothing to write to.");

            return;
        }

        if(state.SoftwareId is null)
        {
            Console.WriteLine($"'{slug}' is not linked to a Software row (status={state.Status}): nothing to write to.");

            return;
        }

        var totals = new Counters();
        await ProcessAsync(context, state, dryRun, totals);
        PrintSummary(totals, dryRun);
    }

    /// <summary>
    ///     Backfills one batch of up to <paramref name="batchSize" /> imported games and stops,
    ///     matching how the other reparse commands interpret <c>--batch-size</c>.
    /// </summary>
    public async Task RunAsync(int batchSize, bool dryRun)
    {
        Console.WriteLine($"Backfilling alternate titles (batch={batchSize}, dryRun={dryRun})\n");

        var totals = new Counters();

        await using MarechaiContext context = await _contextFactory.CreateDbContextAsync();

        List<MobyGamesImportState> batch = await context.MobyGamesImportStates
                                                        .Where(s => s.Status == MobyGamesImportStatus.Imported &&
                                                                    s.SoftwareId != null)
                                                        .OrderBy(s => s.SoftwareId)
                                                        .Take(batchSize)
                                                        .ToListAsync();

        if(batch.Count == 0)
        {
            Console.WriteLine("No imported games to process.");

            return;
        }

        foreach(MobyGamesImportState state in batch)
            await ProcessAsync(context, state, dryRun, totals);

        PrintSummary(totals, dryRun);
    }

    async Task ProcessAsync(MarechaiContext context, MobyGamesImportState state, bool dryRun, Counters totals)
    {
        totals.Processed++;
        Console.Write($"  [Software {state.SoftwareId}] {state.MobyGameId}...");

        try
        {
            List<ParsedAlternateTitle> parsed = await ParseAlternateTitlesForSlugAsync(state.MobyGameId);

            if(parsed.Count == 0)
            {
                Console.WriteLine(" 0 found, skip.");
                totals.SkippedNone++;

                return;
            }

            totals.TitlesFound += parsed.Count;

            Software software = await context.Softwares.FirstOrDefaultAsync(s => s.Id == state.SoftwareId);

            if(software is null)
            {
                Console.WriteLine($" {parsed.Count} found, but Software {state.SoftwareId} is missing, skip.");
                totals.Failed++;

                return;
            }

            AlternateTitleWriter.Result result = await AlternateTitleWriter.ApplyAsync(context, software, parsed);

            if(!result.Changed)
            {
                Console.WriteLine($" {parsed.Count} found, all already present.");
                totals.SkippedUnchanged++;

                return;
            }

            if(dryRun)
                context.ChangeTracker.Clear();
            else
                await context.SaveChangesAsync();

            Console.WriteLine($" {parsed.Count} found, {result.Inserted} new, " +
                              $"{result.CommentsFilled} comments filled{(dryRun ? " (dry-run)" : "")}.");

            totals.Inserted       += result.Inserted;
            totals.CommentsFilled += result.CommentsFilled;
        }
        catch(Exception ex)
        {
            Console.WriteLine($" Error: {ex.Message}");
            totals.Failed++;
            context.ChangeTracker.Clear();
        }
    }

    /// <summary>
    ///     Reassembles the game from its cached raw HTML and returns the alternate titles the Main
    ///     page lists, on either layout. Returns an empty list when nothing is cached for the slug.
    /// </summary>
    async Task<List<ParsedAlternateTitle>> ParseAlternateTitlesForSlugAsync(string slug)
    {
        foreach(string trySlug in MobyGamesSlug.Variants(slug))
        {
            List<MobyGamesRawRow> rows = await _sourceDb.GetRowsForGameAsync(trySlug);

            if(rows.Count == 0) continue;

            ParsedGame game = GameAssembler.Assemble(trySlug, rows);

            return game?.AlternateTitles ?? [];
        }

        return [];
    }

    static void PrintSummary(Counters totals, bool dryRun)
    {
        Console.WriteLine($"\nDone: processed={totals.Processed}, titles_found={totals.TitlesFound}, " +
                          $"{(dryRun ? "would_insert" : "inserted")}={totals.Inserted}, " +
                          $"comments_{(dryRun ? "would_be_filled" : "filled")}={totals.CommentsFilled}, " +
                          $"skipped_none_on_page={totals.SkippedNone}, " +
                          $"skipped_unchanged={totals.SkippedUnchanged}, " +
                          $"failed={totals.Failed}");
    }

    sealed class Counters
    {
        public int CommentsFilled;
        public int Failed;
        public int Inserted;
        public int Processed;
        public int SkippedNone;
        public int SkippedUnchanged;
        public int TitlesFound;
    }
}
