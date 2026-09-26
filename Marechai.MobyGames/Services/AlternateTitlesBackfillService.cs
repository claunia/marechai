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
        Console.WriteLine($"\e[36mBackfilling alternate titles for \e[36;1m{slug}\e[0m" +
                          $"{(dryRun ? " \e[33;1m[DRY RUN]\e[0m" : "")}\n");

        List<ParsedAlternateTitle> found = await ParseAlternateTitlesForSlugAsync(slug);

        Console.WriteLine(found.Count == 0
                              ? "  \e[33mNo alternate titles in cached HTML\e[0m"
                              : $"  \e[32;1m{found.Count}\e[0m alternate title(s) in cached HTML:");

        foreach(ParsedAlternateTitle title in found)
        {
            Console.WriteLine($"    \e[36m{title.Title}\e[0m" +
                              (title.Comment is null ? "" : $" \e[90m— {title.Comment}\e[0m"));
        }

        Console.WriteLine();

        await using MarechaiContext context = await _contextFactory.CreateDbContextAsync();

        MobyGamesImportState state = await context.MobyGamesImportStates
                                                 .FirstOrDefaultAsync(s => s.MobyGameId == slug);

        if(state is null)
        {
            Console.WriteLine($"\e[33mNo import state row for '{slug}': nothing to write to.\e[0m");

            return;
        }

        if(state.SoftwareId is null)
        {
            Console.WriteLine($"\e[33m'{slug}' is not linked to a Software row (status={state.Status}): " +
                              $"nothing to write to.\e[0m");

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
        Console.WriteLine($"\e[36mBackfilling alternate titles \e[90m(batch={batchSize})\e[0m" +
                          $"{(dryRun ? " \e[33;1m[DRY RUN]\e[0m" : "")}\n");

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
            Console.WriteLine("\e[33mNo imported games to process.\e[0m");

            return;
        }

        foreach(MobyGamesImportState state in batch)
            await ProcessAsync(context, state, dryRun, totals);

        PrintSummary(totals, dryRun);
    }

    async Task ProcessAsync(MarechaiContext context, MobyGamesImportState state, bool dryRun, Counters totals)
    {
        totals.Processed++;

        // Most games have no Alternate Titles section at all (roughly five in six), so nothing is
        // printed until there is something to say. The per-game line is written in one go below
        // rather than opened before the parse, and the run summary still counts every game.
        string label = $"  \e[90m[Software {state.SoftwareId}]\e[0m \e[36;1m{state.MobyGameId}\e[0m";

        try
        {
            List<ParsedAlternateTitle> parsed = await ParseAlternateTitlesForSlugAsync(state.MobyGameId);

            if(parsed.Count == 0)
            {
                totals.SkippedNone++;

                return;
            }

            totals.TitlesFound += parsed.Count;

            Software software = await context.Softwares.FirstOrDefaultAsync(s => s.Id == state.SoftwareId);

            if(software is null)
            {
                Console.WriteLine($"{label} \e[33m{parsed.Count} found, but Software {state.SoftwareId} " +
                                  $"is missing, skip\e[0m");
                totals.Failed++;

                return;
            }

            AlternateTitleWriter.Result result = await AlternateTitleWriter.ApplyAsync(context, software, parsed);

            if(!result.Changed)
            {
                totals.SkippedUnchanged++;

                return;
            }

            if(dryRun)
                context.ChangeTracker.Clear();
            else
                await context.SaveChangesAsync();

            Console.WriteLine($"{label} \e[90m{parsed.Count} found\e[0m, " +
                              $"\e[32;1m{result.Inserted}\e[0m new, " +
                              $"{(result.CommentsWritten > 0 ? "\e[32;1m" : "\e[90m")}{result.CommentsWritten}\e[0m " +
                              $"with a comment{(dryRun ? " \e[33m(dry-run)\e[0m" : "")}");

            totals.Inserted        += result.Inserted;
            totals.CommentsWritten += result.CommentsWritten;
        }
        catch(Exception ex)
        {
            Console.WriteLine($"{label} \e[31mError: {ex.Message}\e[0m");
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
        // Counts that represent work carry colour; the quiet ones stay grey so the eye lands on
        // what actually happened. A non-zero failure count is the only thing that turns red.
        string Num(int value, string colourWhenSet) => value > 0 ? $"{colourWhenSet}{value}\e[0m" : $"\e[90m{value}\e[0m";

        Console.WriteLine($"\n  \e[36mDone\e[0m \e[90m— processed\e[0m {Num(totals.Processed, "\e[36;1m")}" +
                          $"\e[90m, titles found\e[0m {Num(totals.TitlesFound, "\e[36;1m")}" +
                          $"\e[90m, {(dryRun ? "would insert" : "inserted")}\e[0m {Num(totals.Inserted, "\e[32;1m")}" +
                          $"\e[90m, with a comment\e[0m {Num(totals.CommentsWritten, "\e[32;1m")}" +
                          $"\e[90m, no titles on page\e[0m {Num(totals.SkippedNone, "\e[90m")}" +
                          $"\e[90m, already complete\e[0m {Num(totals.SkippedUnchanged, "\e[90m")}" +
                          $"\e[90m, failed\e[0m {Num(totals.Failed, "\e[31;1m")}");
    }

    sealed class Counters
    {
        public int CommentsWritten;
        public int Failed;
        public int Inserted;
        public int Processed;
        public int SkippedNone;
        public int SkippedUnchanged;
        public int TitlesFound;
    }
}
