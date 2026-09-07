using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Database.Models;
using Marechai.MobyGames.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.MobyGames.Services;

/// <summary>
///     Audits and repairs <c>SoftwareDescription</c> rows corrupted by the legacy description
///     parser leaking MobyGames page chrome (see <see cref="DescriptionCorruptionDetector" />).
///     Only rows matching the corruption fingerprint are rewritten, so manually edited clean
///     descriptions are never touched. Non-English rows of corrupted software are machine
///     translations of the corrupted text and are deleted so the translation pipeline can
///     regenerate them from the repaired English text.
/// </summary>
public class DescriptionsReparseService
{
    readonly IDbContextFactory<MarechaiContext> _contextFactory;
    readonly SourceDatabaseService              _sourceDb;

    public DescriptionsReparseService(IDbContextFactory<MarechaiContext> contextFactory,
                                      SourceDatabaseService sourceDb)
    {
        _contextFactory = contextFactory;
        _sourceDb       = sourceDb;
    }

    public async Task RunAsync(int batchSize, bool dryRun)
    {
        Console.WriteLine($"Reparsing descriptions (batch={batchSize}, dryRun={dryRun})\n");

        var totals        = new Counters();
        var markerHits    = DescriptionCorruptionDetector.Markers.ToDictionary(m => m, _ => 0);
        var samples       = new List<(ulong SoftwareId, string MobyGameId)>();
        const int maxSamples = 20;

        ulong cursor = 0;
        int   total;

        await using(var countContext = await _contextFactory.CreateDbContextAsync())
        {
            total = await countContext.MobyGamesImportStates
                                      .CountAsync(s => s.Status == MobyGamesImportStatus.Imported &&
                                                       s.SoftwareId != null);
        }

        Console.WriteLine($"{total} imported software-linked games to scan.\n");

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

            double pct = total > 0 ? (totals.Processed + batch.Count) * 100.0 / total : 100;

            Console.WriteLine($"[{totals.Processed + batch.Count}/{total} ({pct:F1}%)] " +
                              $"scanning up to SoftwareId {cursor} " +
                              $"(corrupted so far: {totals.Corrupted})");

            foreach(MobyGamesImportState state in batch)
            {
                totals.Processed++;

                try
                {
                    SoftwareDescription desc = await context.SoftwareDescriptions
                                                            .FirstOrDefaultAsync(d => d.SoftwareId ==
                                                                                     state.SoftwareId &&
                                                                                     d.LanguageCode == "eng");

                    if(desc is null)
                    {
                        totals.SkippedNoDescription++;

                        continue;
                    }

                    List<string> markers = DescriptionCorruptionDetector
                                          .MatchingMarkers(desc.Text, desc.Html)
                                          .ToList();

                    if(markers.Count == 0)
                    {
                        totals.SkippedClean++;

                        continue;
                    }

                    totals.Corrupted++;

                    foreach(string marker in markers)
                        markerHits[marker]++;

                    if(samples.Count < maxSamples) samples.Add((state.SoftwareId.Value, state.MobyGameId));

                    Console.Write($"  [Software {state.SoftwareId}] {state.MobyGameId} corrupted " +
                                  $"({string.Join(", ", markers)})...");

                    ParsedGame game = await ReparseGameAsync(state.MobyGameId);

                    if(game is null || string.IsNullOrWhiteSpace(game.Description))
                    {
                        Console.WriteLine(" no description parsed from raw HTML, leaving row untouched.");
                        totals.FailedReparse++;

                        continue;
                    }

                    if(DescriptionCorruptionDetector.IsCorrupted(game.Description, game.DescriptionHtml))
                    {
                        Console.WriteLine(" reparse output still matches fingerprint, leaving row untouched.");
                        totals.StillCorrupted++;

                        continue;
                    }

                    // Non-eng rows were machine-translated from the corrupted text; delete so
                    // the translation pipeline regenerates them from the repaired English.
                    List<SoftwareDescription> translations = await context.SoftwareDescriptions
                        .Where(d => d.SoftwareId == state.SoftwareId && d.LanguageCode != "eng")
                        .ToListAsync();

                    if(!dryRun)
                    {
                        desc.Text = game.Description;
                        desc.Html = game.DescriptionHtml;

                        if(translations.Count > 0)
                            context.SoftwareDescriptions.RemoveRange(translations);

                        await context.SaveChangesAsync();
                    }

                    totals.Updated++;
                    totals.TranslationsDeleted += translations.Count;

                    Console.WriteLine($" {(dryRun ? "would update" : "updated")}" +
                                      (translations.Count > 0
                                           ? $", {(dryRun ? "would delete" : "deleted")} " +
                                             $"{translations.Count} translation(s)."
                                           : "."));
                }
                catch(Exception ex)
                {
                    Console.WriteLine($" Error: {ex.Message}");
                    totals.Failed++;
                }
            }
        }

        Console.WriteLine($"\nDone: processed={totals.Processed}, corrupted={totals.Corrupted}, " +
                          $"{(dryRun ? "would_update" : "updated")}={totals.Updated}, " +
                          $"skipped_clean={totals.SkippedClean}, " +
                          $"skipped_no_description={totals.SkippedNoDescription}, " +
                          $"still_corrupted={totals.StillCorrupted}, " +
                          $"failed_reparse={totals.FailedReparse}, " +
                          $"translations_{(dryRun ? "would_be_deleted" : "deleted")}={totals.TranslationsDeleted}, " +
                          $"failed={totals.Failed}");

        Console.WriteLine("\nMarker hits:");

        foreach(KeyValuePair<string, int> kv in markerHits.Where(kv => kv.Value > 0).OrderByDescending(kv => kv.Value))
            Console.WriteLine($"  {kv.Key}: {kv.Value}");

        if(samples.Count > 0)
        {
            Console.WriteLine($"\nFirst {samples.Count} corrupted (SoftwareId, MobyGameId):");

            foreach((ulong softwareId, string mobyGameId) in samples)
                Console.WriteLine($"  {softwareId}  {mobyGameId}");
        }
    }

    /// <summary>Reassembles the game from cached raw HTML; returns null when no rows exist.</summary>
    async Task<ParsedGame> ReparseGameAsync(string slug)
    {
        foreach(string trySlug in SlugVariants(slug))
        {
            var rows = await _sourceDb.GetRowsForGameAsync(trySlug);

            if(rows.Count == 0) continue;

            return GameAssembler.Assemble(trySlug, rows);
        }

        return null;
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

    sealed class Counters
    {
        public int Processed;
        public int Corrupted;
        public int Updated;
        public int SkippedClean;
        public int SkippedNoDescription;
        public int StillCorrupted;
        public int FailedReparse;
        public int TranslationsDeleted;
        public int Failed;
    }
}
