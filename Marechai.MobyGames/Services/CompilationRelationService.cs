using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Database.Models;
using Marechai.MobyGames.Parsers;
using Microsoft.EntityFrameworkCore;

namespace Marechai.MobyGames.Services;

public class CompilationRelationService
{
    readonly IDbContextFactory<MarechaiContext> _contextFactory;
    readonly SourceDatabaseService             _sourceDb;
    readonly ImportService                     _importService;

    public CompilationRelationService(IDbContextFactory<MarechaiContext> contextFactory,
                                      SourceDatabaseService sourceDb, ImportService importService)
    {
        _contextFactory = contextFactory;
        _sourceDb       = sourceDb;
        _importService  = importService;
    }

    public async Task RunAsync(int batchSize, bool dryRun)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        // Find "Compilation" genre IDs
        List<int> compilationGenreIds = await context.SoftwareGenres
            .Where(g => g.Name.Contains("Compilation"))
            .Select(g => g.Id)
            .ToListAsync();

        if(compilationGenreIds.Count == 0)
        {
            Console.WriteLine("No 'Compilation' genre found in database.");

            return;
        }

        // Find Software entries with Compilation genre that still have releases pointing to them
        // (i.e., not yet converted to proper compilation releases)
        List<Software> compilationSoftware = await context.Softwares
            .Where(s => context.GenresBySoftware.Any(g => g.SoftwareId == s.Id &&
                                                          compilationGenreIds.Contains(g.GenreId)) &&
                        context.SoftwareReleases.Any(r => r.SoftwareId == s.Id))
            .OrderBy(s => s.Id)
            .Take(batchSize)
            .ToListAsync();

        Console.WriteLine($"Found {compilationSoftware.Count} compilation Software entries to convert " +
                          $"(batch={batchSize})");

        int converted = 0, skipped = 0, failed = 0;

        foreach(Software compilation in compilationSoftware)
        {
            Console.Write($"  [{compilation.Id}] {compilation.Name}...");

            // Find MobyGames import state for this software
            MobyGamesImportState importState = await context.MobyGamesImportStates
                .FirstOrDefaultAsync(s => s.SoftwareId == compilation.Id);

            if(importState is null)
            {
                Console.WriteLine(" No MobyGames import state, skipping.");
                skipped++;

                continue;
            }

            try
            {
                // Get raw HTML from mobygames_raw to extract contained game slugs
                string slug = importState.MobyGameId;

                string[] slugsToTry =
                [
                    slug, $"-{slug}", slug.TrimStart('-'), $"-{slug.TrimStart('-')}"
                ];

                List<string> gameSlugs = null;

                foreach(string trySlug in slugsToTry.Distinct())
                {
                    var rows = await _sourceDb.GetRowsForGameAsync(trySlug);

                    if(rows.Count == 0) continue;

                    // Parse the main tab HTML for game links in description
                    foreach(var row in rows)
                    {
                        var extractedSlugs = MainTabParser.ExtractGameSlugsFromHtml(row.Body, slug);

                        if(extractedSlugs.Count > 0)
                        {
                            gameSlugs = extractedSlugs;

                            break;
                        }
                    }

                    if(gameSlugs is { Count: > 0 }) break;
                }

                if(gameSlugs is null or { Count: 0 })
                {
                    Console.WriteLine(" No game links found in description HTML.");
                    skipped++;

                    continue;
                }

                Console.WriteLine($"\n    Found {gameSlugs.Count} contained game slug(s):");

                foreach(string gameSlug in gameSlugs)
                    Console.WriteLine($"      - {gameSlug}");

                // Resolution phase: resolve all slugs to Software IDs (atomic)
                var containedSoftwareIds = new List<ulong>();
                bool allResolved         = true;

                foreach(string gameSlug in gameSlugs)
                {
                    ulong? softwareId = await ResolveGameSlugAsync(context, gameSlug, dryRun);

                    if(softwareId is null)
                    {
                        Console.WriteLine($"    Cannot resolve '{gameSlug}'. Skipping entire compilation.");
                        allResolved = false;

                        break;
                    }

                    containedSoftwareIds.Add(softwareId.Value);
                    Console.WriteLine($"    Resolved '{gameSlug}' → Software ID: {softwareId}");
                }

                if(!allResolved)
                {
                    skipped++;

                    continue;
                }

                // Conversion phase
                if(dryRun)
                {
                    Console.WriteLine($"    Would convert to compilation with {containedSoftwareIds.Count} game(s)");
                    converted++;

                    continue;
                }

                // Find all SoftwareRelease records pointing to this Software
                var releases = await context.SoftwareReleases
                    .Where(r => r.SoftwareId == compilation.Id)
                    .ToListAsync();

                if(releases.Count == 0)
                {
                    Console.WriteLine("    No releases found for this software, skipping.");
                    skipped++;

                    continue;
                }

                // Convert each release to a compilation release
                foreach(var release in releases)
                {
                    release.IsCompilation = true;
                    release.Title         = compilation.Name;
                    release.SoftwareId    = null;

                    // Add junction entries for contained games
                    foreach(ulong containedId in containedSoftwareIds)
                    {
                        bool junctionExists = await context.SoftwareBySoftwareRelease
                            .AnyAsync(j => j.ReleaseId == release.Id && j.SoftwareId == containedId);

                        if(!junctionExists)
                        {
                            context.SoftwareBySoftwareRelease.Add(new SoftwareBySoftwareRelease
                            {
                                ReleaseId  = release.Id,
                                SoftwareId = containedId
                            });
                        }
                    }
                }

                // Update import state: clear SoftwareId (compilation has no Software)
                importState.SoftwareId = null;

                // Save before deleting the Software (SoftwareRelease FK is Restrict, but we already nulled it)
                await context.SaveChangesAsync();

                // Delete the orphaned Software record
                // Cascade takes care of: GenreBySoftware, PeopleBySoftware, SoftwareDescription,
                // SoftwarePromoArt, SoftwareVideo, SoftwareCriticReview, SoftwareUserRating,
                // SoftwareUserReview, SoftwareVersion, SoftwareScreenshot
                // SoftwareCompanyRole needs explicit deletion (no explicit cascade config)
                var companyRoles = await context.SoftwareCompanyRoles
                    .Where(r => r.SoftwareId == compilation.Id)
                    .ToListAsync();

                context.SoftwareCompanyRoles.RemoveRange(companyRoles);

                context.Softwares.Remove(compilation);
                await context.SaveChangesAsync();

                Console.WriteLine($"    Converted {releases.Count} release(s) to compilation, " +
                                  $"linked {containedSoftwareIds.Count} game(s), " +
                                  $"deleted orphaned Software ID {compilation.Id}");

                converted++;
            }
            catch(Exception ex)
            {
                Console.WriteLine($" Error: {ex.Message}");
                failed++;
            }
        }

        Console.WriteLine($"\nDone: {converted} converted, {skipped} skipped, {failed} failed");
    }

    /// <summary>
    ///     Resolves a MobyGames game slug to a Software ID using local database only.
    ///     Tries all slug variants (with/without leading dash) in MobyGamesImportState,
    ///     then attempts to import from mobygames_raw if not found.
    /// </summary>
    async Task<ulong?> ResolveGameSlugAsync(MarechaiContext context, string slug, bool dryRun)
    {
        if(string.IsNullOrWhiteSpace(slug)) return null;

        string trimmed = slug.TrimStart('-');

        string[] slugsToTry = [slug, $"-{slug}", trimmed, $"-{trimmed}"];

        // Try MobyGamesImportState lookup
        foreach(string trySlug in slugsToTry.Distinct())
        {
            MobyGamesImportState state = await context.MobyGamesImportStates
                .FirstOrDefaultAsync(s => s.MobyGameId == trySlug &&
                                          s.Status == MobyGamesImportStatus.Imported);

            if(state?.SoftwareId is not null) return state.SoftwareId;
        }

        // Try importing from mobygames_raw
        foreach(string trySlug in slugsToTry.Distinct())
        {
            var rows = await _sourceDb.GetRowsForGameAsync(trySlug);

            if(rows.Count > 0)
            {
                if(dryRun)
                {
                    Console.Write($" [dry-run: would import '{trySlug}']");

                    return 0; // placeholder for dry-run
                }

                Console.Write($"      Importing '{trySlug}' from mobygames_raw...");
                ulong? importedId = await _importService.ImportGameBySlugAsync(trySlug);

                if(importedId is not null)
                {
                    Console.WriteLine($" OK (ID: {importedId})");

                    return importedId;
                }

                Console.WriteLine(" failed");
            }
        }

        return null;
    }
}
