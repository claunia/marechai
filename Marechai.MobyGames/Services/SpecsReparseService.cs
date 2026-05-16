using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Marechai.Data;
using Marechai.Database.Models;
using Marechai.MobyGames.Models;
using Marechai.MobyGames.Parsers;
using Microsoft.EntityFrameworkCore;

namespace Marechai.MobyGames.Services;

/// <summary>
///     Reparses the Specs tab from <c>mobygames_raw</c> for already-imported games and
///     replaces their <c>SoftwareAttribute</c> rows of category "Spec" with one row per
///     anchor (instead of a single concatenated value). Skips writes when the parsed
///     content matches the existing rows (idempotent re-runs).
/// </summary>
public class SpecsReparseService
{
    readonly IDbContextFactory<MarechaiContext> _contextFactory;
    readonly SourceDatabaseService              _sourceDb;

    public SpecsReparseService(IDbContextFactory<MarechaiContext> contextFactory, SourceDatabaseService sourceDb)
    {
        _contextFactory = contextFactory;
        _sourceDb       = sourceDb;
    }

    public async Task RunAsync(int batchSize, bool dryRun)
    {
        Console.WriteLine($"Reparsing Specs (batch={batchSize}, dryRun={dryRun})\n");

        var totals = new Counters();

        Console.WriteLine("== Pass A: software-scoped imports ==");
        await RunPassAAsync(batchSize, dryRun, totals);

        Console.WriteLine("\n== Pass B: compilation-scoped imports ==");
        await RunPassBAsync(batchSize, dryRun, totals);

        Console.WriteLine($"\nDone: processed={totals.Processed}, updated={totals.Updated}, " +
                          $"skipped_unchanged={totals.SkippedUnchanged}, " +
                          $"skipped_no_specs={totals.SkippedNoSpecs}, " +
                          $"skipped_no_compilation_release={totals.SkippedNoCompilationRelease}, " +
                          $"failed={totals.Failed}");
    }

    async Task RunPassAAsync(int batchSize, bool dryRun, Counters totals)
    {
        ulong cursor = 0;

        while(true)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            List<MobyGamesImportState> batch = await context.MobyGamesImportStates
                                                            .Where(s => s.Status == MobyGamesImportStatus.Imported &&
                                                                        s.SoftwareId != null && s.SoftwareId > cursor)
                                                            .OrderBy(s => s.SoftwareId)
                                                            .Take(batchSize)
                                                            .ToListAsync();

            if(batch.Count == 0) break;

            cursor = batch[^1].SoftwareId!.Value;

            foreach(MobyGamesImportState state in batch)
            {
                totals.Processed++;
                Console.Write($"  [Software {state.SoftwareId}] {state.MobyGameId}...");

                try
                {
                    List<ParsedSpec> parsedSpecs = await ParseSpecsForSlugAsync(state.MobyGameId);

                    if(parsedSpecs.Count == 0)
                    {
                        Console.WriteLine(" no specs in raw HTML, skip.");
                        totals.SkippedNoSpecs++;

                        continue;
                    }

                    // Eagerly load releases for this software with platform + spec attributes
                    List<SoftwareRelease> releases = await context.SoftwareReleases
                                                                  .Include(r => r.Platform)
                                                                  .Include(r => r.Attributes)
                                                                  .Where(r => r.SoftwareId == state.SoftwareId)
                                                                  .ToListAsync();

                    int releasesUpdated     = 0;
                    int releasesUnchanged   = 0;
                    int releasesNoMatch     = 0;

                    foreach(SoftwareRelease release in releases)
                    {
                        string platformName = release.Platform?.Name;

                        if(string.IsNullOrEmpty(platformName))
                        {
                            releasesNoMatch++;

                            continue;
                        }

                        List<ParsedSpec> matchingSpecs = parsedSpecs
                                                       .Where(s => s.Platform == platformName)
                                                       .ToList();

                        if(matchingSpecs.Count == 0)
                        {
                            releasesNoMatch++;

                            continue;
                        }

                        if(ApplySpecsToRelease(context, release, matchingSpecs, dryRun))
                            releasesUpdated++;
                        else
                            releasesUnchanged++;
                    }

                    if(releasesUpdated == 0 && releasesUnchanged > 0)
                    {
                        Console.WriteLine($" unchanged ({releasesUnchanged} release(s)).");
                        totals.SkippedUnchanged++;

                        continue;
                    }

                    if(releasesUpdated == 0)
                    {
                        Console.WriteLine(" no matching platforms in DB, skip.");
                        totals.SkippedNoSpecs++;

                        continue;
                    }

                    if(!dryRun) await context.SaveChangesAsync();

                    Console.WriteLine($" {(dryRun ? "would update" : "updated")} " +
                                      $"{releasesUpdated} release(s), unchanged {releasesUnchanged}.");
                    totals.Updated++;
                }
                catch(Exception ex)
                {
                    Console.WriteLine($" Error: {ex.Message}");
                    totals.Failed++;
                }
            }
        }
    }

    async Task RunPassBAsync(int batchSize, bool dryRun, Counters totals)
    {
        long cursor = 0;

        while(true)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            List<MobyGamesImportState> batch = await context.MobyGamesImportStates
                                                            .Where(s => s.Status == MobyGamesImportStatus.Imported &&
                                                                        s.SoftwareId == null && s.Id > cursor)
                                                            .OrderBy(s => s.Id)
                                                            .Take(batchSize)
                                                            .ToListAsync();

            if(batch.Count == 0) break;

            cursor = batch[^1].Id;

            foreach(MobyGamesImportState state in batch)
            {
                totals.Processed++;
                Console.Write($"  [Compilation state {state.Id}] {state.MobyGameId}...");

                try
                {
                    (List<ParsedSpec> parsedSpecs, string title) = await ParseSpecsAndTitleForSlugAsync(state.MobyGameId);

                    if(parsedSpecs.Count == 0)
                    {
                        Console.WriteLine(" no specs in raw HTML, skip.");
                        totals.SkippedNoSpecs++;

                        continue;
                    }

                    if(string.IsNullOrWhiteSpace(title))
                    {
                        Console.WriteLine(" no title found in main tab, skip.");
                        totals.SkippedNoCompilationRelease++;

                        continue;
                    }

                    List<SoftwareRelease> matches = await context.SoftwareReleases
                                                                 .Include(r => r.Attributes)
                                                                 .Where(r => r.IsCompilation && r.Title == title)
                                                                 .ToListAsync();

                    if(matches.Count == 0)
                    {
                        Console.WriteLine($" no compilation release with Title='{title}', skip.");
                        totals.SkippedNoCompilationRelease++;

                        continue;
                    }

                    if(matches.Count > 1)
                    {
                        Console.WriteLine($" ambiguous: {matches.Count} compilation releases with " +
                                          $"Title='{title}', skip.");
                        totals.SkippedNoCompilationRelease++;

                        continue;
                    }

                    SoftwareRelease release = matches[0];

                    // Compilations are not bound to a single platform; flatten across platforms.
                    List<ParsedSpec> flattened = parsedSpecs
                                               .Select(s => new ParsedSpec
                                                {
                                                    Platform = null,
                                                    Key      = s.Key,
                                                    Value    = s.Value
                                                })
                                               .ToList();

                    bool changed = ApplySpecsToRelease(context, release, flattened, dryRun);

                    if(!changed)
                    {
                        Console.WriteLine(" unchanged.");
                        totals.SkippedUnchanged++;

                        continue;
                    }

                    if(!dryRun) await context.SaveChangesAsync();

                    Console.WriteLine($" {(dryRun ? "would update" : "updated")} 1 compilation release.");
                    totals.Updated++;
                }
                catch(Exception ex)
                {
                    Console.WriteLine($" Error: {ex.Message}");
                    totals.Failed++;
                }
            }
        }
    }

    async Task<List<ParsedSpec>> ParseSpecsForSlugAsync(string slug)
    {
        var aggregated = new List<ParsedSpec>();

        foreach(string trySlug in SlugVariants(slug))
        {
            var rows = await _sourceDb.GetRowsForGameAsync(trySlug);

            if(rows.Count == 0) continue;

            foreach(MobyGamesRawRow row in rows)
            {
                if(TabDetector.Detect(row.Body) != MobyTab.Specs) continue;

                var doc = new HtmlDocument();
                doc.LoadHtml(row.Body);

                var game = new ParsedGame { MobyGameId = trySlug };
                SpecsTabParser.Parse(doc, game);
                aggregated.AddRange(game.Specs);
            }

            if(aggregated.Count > 0) return aggregated;
        }

        return aggregated;
    }

    async Task<(List<ParsedSpec> Specs, string Title)> ParseSpecsAndTitleForSlugAsync(string slug)
    {
        var aggregated = new List<ParsedSpec>();
        string title   = null;

        foreach(string trySlug in SlugVariants(slug))
        {
            var rows = await _sourceDb.GetRowsForGameAsync(trySlug);

            if(rows.Count == 0) continue;

            foreach(MobyGamesRawRow row in rows)
            {
                MobyTab tab = TabDetector.Detect(row.Body);

                if(tab == MobyTab.Specs)
                {
                    var doc = new HtmlDocument();
                    doc.LoadHtml(row.Body);

                    var game = new ParsedGame { MobyGameId = trySlug };
                    SpecsTabParser.Parse(doc, game);
                    aggregated.AddRange(game.Specs);
                }
                else if(tab == MobyTab.Main && title is null)
                {
                    var doc = new HtmlDocument();
                    doc.LoadHtml(row.Body);

                    var game = new ParsedGame { MobyGameId = trySlug };
                    MainTabParser.Parse(doc, game);
                    title = game.Name;
                }
            }

            if(aggregated.Count > 0 || title is not null) return (aggregated, title);
        }

        return (aggregated, title);
    }

    static IEnumerable<string> SlugVariants(string slug)
    {
        if(string.IsNullOrWhiteSpace(slug)) yield break;

        string trimmed = slug.TrimStart('-');
        var seen       = new HashSet<string>();

        foreach(string candidate in new[] { slug, $"-{slug}", trimmed, $"-{trimmed}" })
            if(seen.Add(candidate))
                yield return candidate;
    }

    /// <summary>
    ///     Compares parsed specs against existing ones for a release; if equal, returns false
    ///     (no DB writes queued). Otherwise removes existing Spec attributes and adds new ones,
    ///     returning true. Caller is responsible for SaveChangesAsync.
    /// </summary>
    static bool ApplySpecsToRelease(MarechaiContext context, SoftwareRelease release,
                                    IReadOnlyList<ParsedSpec> parsedSpecs, bool dryRun)
    {
        var existing = release.Attributes
                              .Where(a => a.Category == "Spec")
                              .Select(a => (a.Key, a.Value))
                              .OrderBy(p => p.Key, StringComparer.Ordinal)
                              .ThenBy(p => p.Value, StringComparer.Ordinal)
                              .ToList();

        var incoming = parsedSpecs
                      .Select(s => (s.Key, s.Value))
                      .OrderBy(p => p.Key, StringComparer.Ordinal)
                      .ThenBy(p => p.Value, StringComparer.Ordinal)
                      .ToList();

        if(existing.SequenceEqual(incoming)) return false;

        if(dryRun) return true;

        var toRemove = release.Attributes.Where(a => a.Category == "Spec").ToList();
        context.SoftwareAttributes.RemoveRange(toRemove);

        foreach(ParsedSpec spec in parsedSpecs)
        {
            // MobyGames serves cells like `3D&nbsp;Accelerator`; HtmlDecode turns &nbsp; into U+00A0,
            // and MariaDB's utf8mb4_*_ci collations treat U+00A0 != U+0020, breaking exact-match
            // search. Normalise NBSP -> regular space on the way into the DB.
            context.SoftwareAttributes.Add(new SoftwareAttribute
            {
                SoftwareReleaseId = release.Id,
                Category          = "Spec",
                Key               = spec.Key.Replace('\u00A0',   ' '),
                Value             = spec.Value.Replace('\u00A0', ' ')
            });
        }

        return true;
    }

    sealed class Counters
    {
        public int Processed;
        public int Updated;
        public int SkippedUnchanged;
        public int SkippedNoSpecs;
        public int SkippedNoCompilationRelease;
        public int Failed;
    }
}
