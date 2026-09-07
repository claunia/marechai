using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Database.Models;
using Marechai.MobyGames.Models;
using Marechai.MobyGames.Parsers;
using Microsoft.EntityFrameworkCore;
using NewSite = Marechai.MobyGames.Parsers.NewSite;

namespace Marechai.MobyGames.Services;

/// <summary>
///     Reparses the Covers tab from <c>mobygames_raw</c> for games with existing
///     <see cref="MobyGamesCoverDownloadState" /> rows and corrects their <c>GroupId</c>/
///     <c>Platform</c> in place. This is a data-repair pass for state rows captured before
///     a parser bug (old-layout <see cref="CoverArtTabParser" /> matching the first
///     "add covers" sidebar link on the whole page instead of the one scoped to the current
///     cover-art heading) collapsed every group on a page onto the same GroupId. No network
///     access is used - HTML comes from the already-cached source DB. Run
///     <c>repair-covers</c> afterwards to propagate the corrected GroupId/Platform onto
///     <see cref="SoftwareCover" />/<see cref="SoftwareCoverGroup" />.
/// </summary>
public class CoversReparseService
{
    readonly IDbContextFactory<MarechaiContext> _contextFactory;
    readonly SourceDatabaseService              _sourceDb;

    public CoversReparseService(IDbContextFactory<MarechaiContext> contextFactory, SourceDatabaseService sourceDb)
    {
        _contextFactory = contextFactory;
        _sourceDb       = sourceDb;
    }

    public async Task RunAsync(int batchSize, bool dryRun)
    {
        Console.WriteLine($"Reparsing Covers (batch={batchSize}, dryRun={dryRun})\n");

        int processedSlugs = 0;
        int updatedStates  = 0;
        int skippedNoHtml  = 0;
        int unmatched      = 0;
        int failed         = 0;

        long cursor = 0;

        while(true)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            List<MobyGamesCoverDownloadState> batch = await context.MobyGamesCoverDownloadStates
                                                                    .Where(s => s.Id > cursor)
                                                                    .OrderBy(s => s.Id)
                                                                    .Take(batchSize)
                                                                    .ToListAsync();

            if(batch.Count == 0) break;

            cursor = batch[^1].Id;

            foreach(string slug in batch.Select(s => s.MobyGameId).Distinct())
            {
                processedSlugs++;
                Console.Write($"  [{slug}]...");

                try
                {
                    List<ParsedCoverGroup> groups = await ParseCoverGroupsForSlugAsync(slug);

                    if(groups.Count == 0)
                    {
                        Console.WriteLine(" no covers tab in raw HTML, skip.");
                        skippedNoHtml++;

                        continue;
                    }

                    // Map DetailPageUrl -> (GroupId, Platform) from the freshly (correctly) parsed HTML.
                    var byUrl = new Dictionary<string, (string GroupId, string Platform)>();

                    foreach(ParsedCoverGroup group in groups)
                    foreach(ParsedCoverImage cover in group.Covers)
                        byUrl[cover.DetailPageUrl] = (group.GroupId, group.Platform);

                    List<MobyGamesCoverDownloadState> states = await context.MobyGamesCoverDownloadStates
                                                                             .Where(s => s.MobyGameId == slug)
                                                                             .ToListAsync();

                    int changedHere = 0;

                    foreach(MobyGamesCoverDownloadState state in states)
                    {
                        if(!byUrl.TryGetValue(state.CoverPageUrl, out (string GroupId, string Platform) parsed))
                        {
                            unmatched++;

                            continue;
                        }

                        if(state.GroupId == parsed.GroupId && state.Platform == parsed.Platform) continue;

                        Console.Write($"\n    {state.CoverPageUrl}: group {state.GroupId ?? "(none)"} -> " +
                                      $"{parsed.GroupId ?? "(none)"}, platform {state.Platform ?? "(none)"} -> " +
                                      $"{parsed.Platform ?? "(none)"}");

                        if(!dryRun)
                        {
                            state.GroupId  = parsed.GroupId;
                            state.Platform = parsed.Platform;
                        }

                        changedHere++;
                    }

                    if(changedHere == 0)
                    {
                        Console.WriteLine(" unchanged.");

                        continue;
                    }

                    if(!dryRun) await context.SaveChangesAsync();

                    Console.WriteLine($"\n    {(dryRun ? "would update" : "updated")} {changedHere} state row(s).");
                    updatedStates += changedHere;
                }
                catch(Exception ex)
                {
                    Console.WriteLine($" Error: {ex.Message}");
                    failed++;
                }
            }
        }

        Console.WriteLine($"\nDone: slugs_processed={processedSlugs}, states_updated={updatedStates}, " +
                          $"skipped_no_html={skippedNoHtml}, unmatched={unmatched}, failed={failed}");

        if(!dryRun && updatedStates > 0)
            Console.WriteLine("\n  Run 'repair-covers' next to propagate the corrected GroupId/Platform " +
                              "onto SoftwareCover/SoftwareCoverGroup.");
    }

    async Task<List<ParsedCoverGroup>> ParseCoverGroupsForSlugAsync(string slug)
    {
        foreach(string trySlug in SlugVariants(slug))
        {
            List<MobyGamesRawRow> rows = await _sourceDb.GetRowsForGameAsync(trySlug);

            if(rows.Count == 0) continue;

            foreach(MobyGamesRawRow row in rows)
            {
                (MobyTab tab, MobyLayout layout) = TabDetector.DetectWithLayout(row.Body);

                if(tab != MobyTab.CoverArt) continue;

                return layout == MobyLayout.New
                           ? NewSite.CoverArtTabParser.Parse(row.Body)
                           : CoverArtTabParser.Parse(row.Body);
            }
        }

        return [];
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
}
