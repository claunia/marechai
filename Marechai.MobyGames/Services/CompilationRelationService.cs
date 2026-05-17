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

public class CompilationRelationService
{
    readonly IDbContextFactory<MarechaiContext> _contextFactory;
    readonly SourceDatabaseService             _sourceDb;
    readonly ImportService                     _importService;
    readonly AdminMessageService               _adminMessenger;

    public CompilationRelationService(IDbContextFactory<MarechaiContext> contextFactory,
                                      SourceDatabaseService sourceDb, ImportService importService,
                                      AdminMessageService adminMessenger = null)
    {
        _contextFactory = contextFactory;
        _sourceDb       = sourceDb;
        _importService  = importService;
        _adminMessenger = adminMessenger;
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

        int converted = 0, partial = 0, skipped = 0, failed = 0;

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
                // Get raw HTML from mobygames_raw to extract contained game slugs and unresolvable anchors.
                string slug = importState.MobyGameId;

                string[] slugsToTry =
                [
                    slug, $"-{slug}", slug.TrimStart('-'), $"-{slug.TrimStart('-')}"
                ];

                List<string>                       gameSlugs    = null;
                List<UnresolvableCompilationLink>  unresolvable = null;

                foreach(string trySlug in slugsToTry.Distinct())
                {
                    var rows = await _sourceDb.GetRowsForGameAsync(trySlug);

                    if(rows.Count == 0) continue;

                    // Parse the main tab HTML for game links AND unresolvable anchors in the description.
                    foreach(var row in rows)
                    {
                        (List<string> extractedSlugs, List<UnresolvableCompilationLink> extractedUnresolvable) =
                            MainTabParser.ExtractCompilationContentsFromHtml(row.Body, slug);

                        if(extractedSlugs.Count > 0 || extractedUnresolvable.Count > 0)
                        {
                            gameSlugs    = extractedSlugs;
                            unresolvable = extractedUnresolvable;

                            break;
                        }
                    }

                    if((gameSlugs is { Count: > 0 }) || (unresolvable is { Count: > 0 })) break;
                }

                gameSlugs    ??= [];
                unresolvable ??= [];

                if(gameSlugs.Count == 0 && unresolvable.Count == 0)
                {
                    Console.WriteLine(" No game links found in description HTML.");
                    skipped++;

                    continue;
                }

                Console.WriteLine($"\n    Found {gameSlugs.Count} contained game slug(s) and " +
                                  $"{unresolvable.Count} unresolvable anchor(s):");

                foreach(string gameSlug in gameSlugs)
                    Console.WriteLine($"      - {gameSlug}");

                foreach(UnresolvableCompilationLink link in unresolvable)
                    Console.WriteLine($"      ? {link.Name}  →  {link.Href}");

                // Resolution phase: resolve every slug we can but DO NOT abort on individual failures.
                var containedSoftwareIds = new List<ulong>();
                var unresolvedSlugs      = new List<string>();

                foreach(string gameSlug in gameSlugs)
                {
                    ulong? softwareId = await ResolveGameSlugAsync(context, gameSlug, dryRun);

                    if(softwareId is null)
                    {
                        Console.WriteLine($"    WARNING: Cannot resolve '{gameSlug}'.");
                        unresolvedSlugs.Add(gameSlug);

                        continue;
                    }

                    containedSoftwareIds.Add(softwareId.Value);
                    Console.WriteLine($"    Resolved '{gameSlug}' → Software ID: {softwareId}");
                }

                bool hasUnresolved      = unresolvedSlugs.Count > 0 || unresolvable.Count > 0;
                bool compilationCreated = false;

                if(containedSoftwareIds.Count == 0)
                {
                    Console.WriteLine("    FAILED: No contained games could be resolved; compilation not created.");

                    if(hasUnresolved && !dryRun && _adminMessenger is not null)
                        await _adminMessenger.SendCompilationReportAsync(compilation.Name, importState.MobyGameId,
                                                                         resolvedSoftwareIds: [],
                                                                         unresolvedSlugs: unresolvedSlugs,
                                                                         unresolvableLinks: unresolvable,
                                                                         compilationCreated: false);
                    else if(hasUnresolved)
                        Console.WriteLine("    [dry-run] Would send admin report (compilation not created).");

                    skipped++;

                    continue;
                }

                // Conversion phase
                if(dryRun)
                {
                    Console.WriteLine($"    Would convert to compilation with {containedSoftwareIds.Count} game(s)" +
                                      (hasUnresolved
                                           ? $" and send admin report ({unresolvedSlugs.Count} unresolved slug(s), " +
                                             $"{unresolvable.Count} unresolvable anchor(s))"
                                           : ""));

                    if(hasUnresolved) partial++;
                    else converted++;

                    continue;
                }

                try
                {
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

                    // Update import state: clear SoftwareId (compilation has no Software).
                    // LEGITIMATE null-out: this is one of only two places allowed to clear
                    // MobyGamesImportState.SoftwareId. It is paired with the Softwares.Remove(compilation)
                    // call below, so the slug<->Software invariant holds (the Software is going away).
                    // See MarkFailedAsync / MarkRejectedAsync in StateService.cs which intentionally
                    // preserve SoftwareId on every other path.
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

                    compilationCreated = true;

                    if(hasUnresolved)
                    {
                        Console.WriteLine($"    Converted {releases.Count} release(s) to PARTIAL compilation, " +
                                          $"linked {containedSoftwareIds.Count} game(s), " +
                                          $"{unresolvedSlugs.Count} unresolved slug(s), " +
                                          $"{unresolvable.Count} unresolvable anchor(s), " +
                                          $"deleted orphaned Software ID {compilation.Id}");

                        partial++;
                    }
                    else
                    {
                        Console.WriteLine($"    Converted {releases.Count} release(s) to compilation, " +
                                          $"linked {containedSoftwareIds.Count} game(s), " +
                                          $"deleted orphaned Software ID {compilation.Id}");

                        converted++;
                    }
                }
                finally
                {
                    // Always send the partial-compilation report when there were unresolved entries — even if
                    // a downstream mutation throws (we'll re-raise via the outer catch which logs Failed).
                    if(hasUnresolved && _adminMessenger is not null)
                    {
                        await _adminMessenger.SendCompilationReportAsync(compilation.Name, importState.MobyGameId,
                                                                         resolvedSoftwareIds: containedSoftwareIds,
                                                                         unresolvedSlugs: unresolvedSlugs,
                                                                         unresolvableLinks: unresolvable,
                                                                         compilationCreated: compilationCreated);
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($" Error: {ex.Message}");
                failed++;
            }
        }

        Console.WriteLine($"\nDone: {converted} converted, {partial} partial, {skipped} skipped, {failed} failed");
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
