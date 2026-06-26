using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.MobyGames.Services;

/// <summary>
///     Cleans up <see cref="Software" /> orphans created by interrupted MobyGames imports
///     under <c>--yes-to-all</c> mode. An orphan is a Software whose row is NOT referenced by
///     any <see cref="MobyGamesImportState" />, but which shares its name with another
///     Software that IS state-linked. The state-linked twin is the canonical row (preserves
///     the slug<->Software mapping); the orphan's child data is merged into it and the
///     orphan deleted.
///     <para>
///         Required ONLY for backfill of historical data created before
///         <see cref="StateService.MarkSoftwareLinkedAsync" /> was introduced. With the new
///         state-row-first invariant in place, no new orphans of this shape are expected.
///     </para>
/// </summary>
public class OrphanCleanupService
{
    readonly IDbContextFactory<MarechaiContext> _contextFactory;

    public OrphanCleanupService(IDbContextFactory<MarechaiContext> contextFactory) =>
        _contextFactory = contextFactory;

    public async Task RunAsync(bool dryRun, bool yesToAll)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        Console.WriteLine("  Scanning for orphan Software rows that duplicate a state-linked twin...");

        // 1. Build the set of Software IDs referenced by any state row.
        var stateLinkedIds = await context.MobyGamesImportStates
                                          .Where(s => s.SoftwareId != null)
                                          .Select(s => s.SoftwareId.Value)
                                          .Distinct()
                                          .ToListAsync();

        if(stateLinkedIds.Count == 0)
        {
            Console.WriteLine("  No state-linked Software rows found. Nothing to clean up.");

            return;
        }

        var stateLinkedSet = new HashSet<ulong>(stateLinkedIds);

        // 2. Pull every Software (Id, Name) into memory and group by name. For each group
        //    containing AT LEAST one linked Software, every unlinked Software is an orphan
        //    candidate paired with that group's lone twin.
        var nameIndex = await context.Softwares
                                     .Select(s => new { s.Id, s.Name })
                                     .ToListAsync();

        var candidates = new List<(ulong OrphanId, ulong TwinId, string Name)>();
        var skipped    = new List<(string Name, int LinkedCount, int OrphanCount)>();

        foreach(var grp in nameIndex.GroupBy(g => g.Name, StringComparer.Ordinal))
        {
            var linked   = grp.Where(g => stateLinkedSet.Contains(g.Id)).Select(g => g.Id).ToList();
            var unlinked = grp.Where(g => !stateLinkedSet.Contains(g.Id)).Select(g => g.Id).ToList();

            if(linked.Count == 0 || unlinked.Count == 0) continue;

            if(linked.Count > 1)
            {
                skipped.Add((grp.Key, linked.Count, unlinked.Count));

                continue;
            }

            ulong twinId = linked[0];

            foreach(ulong orphanId in unlinked)
                candidates.Add((orphanId, twinId, grp.Key));
        }

        Console.WriteLine($"  Found {candidates.Count} orphan candidate(s).");

        if(skipped.Count > 0)
        {
            Console.WriteLine($"  Skipped {skipped.Count} ambiguous name group(s) with multiple state-linked twins:");

            foreach(var s in skipped.Take(20))
                Console.WriteLine($"    \"{s.Name}\": {s.LinkedCount} linked, {s.OrphanCount} unlinked");

            if(skipped.Count > 20) Console.WriteLine($"    ...and {skipped.Count - 20} more.");
        }

        if(candidates.Count == 0) return;

        if(dryRun)
        {
            Console.WriteLine("\n  DRY RUN — no changes will be made.");

            foreach(var c in candidates.Take(50))
                Console.WriteLine($"    Would merge orphan {c.OrphanId} -> twin {c.TwinId}  \"{c.Name}\"");

            if(candidates.Count > 50) Console.WriteLine($"    ...and {candidates.Count - 50} more.");

            return;
        }

        if(!yesToAll)
        {
            Console.Write($"\n  Merge {candidates.Count} orphan(s) into their twins? [y/N]: ");
            string answer = Console.ReadLine()?.Trim();

            if(!string.Equals(answer, "y", StringComparison.OrdinalIgnoreCase) &&
               !string.Equals(answer, "yes", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("  Aborted.");

                return;
            }
        }

        int merged = 0, failed = 0;

        foreach(var c in candidates)
        {
            Console.Write($"  Merging orphan {c.OrphanId} -> twin {c.TwinId}  \"{c.Name}\"... ");

            try
            {
                await MergeOneAsync(c.OrphanId, c.TwinId);
                merged++;
                Console.WriteLine("done.");
            }
            catch(Exception ex)
            {
                failed++;
                Console.WriteLine($"FAILED: {ex.Message}");
            }
        }

        Console.WriteLine($"\n  Merged {merged} orphan(s), {failed} failed.");
    }

    /// <summary>
    ///     For each child entity referencing <paramref name="orphanId" />: either redirect the
    ///     row to <paramref name="twinId" /> (if no conflicting row exists on the twin) or
    ///     delete the orphan's row (if the twin already has a content-equivalent or it would
    ///     violate a unique/PK constraint). Finally deletes the orphan <see cref="Software" /> row.
    ///     <para>
    ///         Releases (and versions) are content-checked for duplication first: a release with
    ///         the same (Title, PlatformId, PublisherId, ReleaseDate) tuple on the twin causes
    ///         the orphan's release to be deleted (cascade-deleting all its children) so we
    ///         don't end up with twin owning two copies of the same release. Junction tables
    ///         (descriptions, genres, company roles, etc.) are deduped by their natural key.
    ///         Other one-to-many media rows (screenshots, promo art, videos, reviews) have no
    ///         reliable dedup key and are simply redirected; deduplication of those should be a
    ///         separate exercise.
    ///     </para>
    ///     <para>
    ///         All junction updates use <c>ExecuteDelete</c>/<c>ExecuteUpdate</c> rather than
    ///         tracked-entity modifications. This is required for composite-PK tables
    ///         (<see cref="GenreBySoftware" />, <see cref="SoftwareCompanyRole" />,
    ///         <see cref="SoftwareBySoftwareRelease" />) whose <c>SoftwareId</c> is part of the
    ///         primary key — EF's change tracker rejects in-place PK column modification with
    ///         "the property is part of a key and so cannot be modified or marked as modified".
    ///         For consistency and speed, the same pattern is used for the other junctions.
    ///     </para>
    /// </summary>
    async Task MergeOneAsync(ulong orphanId, ulong twinId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        // ====================================================================
        // 1. SoftwareReleases — delete duplicates, redirect the rest.
        // ====================================================================
        // A release on the orphan is a duplicate of a release on the twin when
        // (Title, PlatformId, PublisherId, ReleaseDate) all match. Duplicate
        // releases on the orphan are DELETED (their children — Barcodes,
        // ProductCodes, Covers, Attributes, SoftwareBySoftwareRelease rows,
        // Languages, Regions, Gpus, SoundSynths, IncludedVersions — all
        // cascade-delete automatically per the OnDelete(Cascade) FK config).
        // Non-duplicate releases are redirected to the twin so the data isn't
        // lost.
        await context.SoftwareReleases
                     .Where(r => r.SoftwareId == orphanId &&
                                 context.SoftwareReleases.Any(t => t.SoftwareId  == twinId        &&
                                                                   t.Title       == r.Title       &&
                                                                   t.PlatformId  == r.PlatformId  &&
                                                                   t.PublisherId == r.PublisherId &&
                                                                   t.ReleaseDate == r.ReleaseDate))
                     .ExecuteDeleteAsync();

        await context.SoftwareReleases.Where(r => r.SoftwareId == orphanId)
                     .ExecuteUpdateAsync(s => s.SetProperty(r => r.SoftwareId, _ => (ulong?)twinId));

        // ====================================================================
        // 2. SoftwareDescription — unique per (SoftwareId, LanguageCode).
        // ====================================================================
        await context.SoftwareDescriptions
                     .Where(d => d.SoftwareId == orphanId &&
                                 context.SoftwareDescriptions.Any(t => t.SoftwareId   == twinId &&
                                                                       t.LanguageCode == d.LanguageCode))
                     .ExecuteDeleteAsync();

        await context.SoftwareDescriptions.Where(d => d.SoftwareId == orphanId)
                     .ExecuteUpdateAsync(s => s.SetProperty(d => d.SoftwareId, _ => twinId));

        // ====================================================================
        // 3. GenreBySoftware — composite PK (SoftwareId, GenreId).
        // ====================================================================
        await context.GenresBySoftware
                     .Where(g => g.SoftwareId == orphanId &&
                                 context.GenresBySoftware.Any(t => t.SoftwareId == twinId && t.GenreId == g.GenreId))
                     .ExecuteDeleteAsync();

        await context.GenresBySoftware.Where(g => g.SoftwareId == orphanId)
                     .ExecuteUpdateAsync(s => s.SetProperty(g => g.SoftwareId, _ => twinId));

        // ====================================================================
        // 4. SoftwareCompanyRole — composite PK (SoftwareId, CompanyId, RoleId).
        // ====================================================================
        await context.SoftwareCompanyRoles
                     .Where(r => r.SoftwareId == orphanId &&
                                 context.SoftwareCompanyRoles.Any(t => t.SoftwareId == twinId      &&
                                                                       t.CompanyId  == r.CompanyId &&
                                                                       t.RoleId     == r.RoleId))
                     .ExecuteDeleteAsync();

        await context.SoftwareCompanyRoles.Where(r => r.SoftwareId == orphanId)
                     .ExecuteUpdateAsync(s => s.SetProperty(r => r.SoftwareId, _ => twinId));

        // ====================================================================
        // 5. PeopleBySoftware — own Id, dedup conservatively by (PersonId, RoleId).
        // ====================================================================
        await context.PeopleBySoftware
                     .Where(p => p.SoftwareId == orphanId &&
                                 context.PeopleBySoftware.Any(t => t.SoftwareId == twinId     &&
                                                                   t.PersonId   == p.PersonId &&
                                                                   t.RoleId     == p.RoleId))
                     .ExecuteDeleteAsync();

        await context.PeopleBySoftware.Where(p => p.SoftwareId == orphanId)
                     .ExecuteUpdateAsync(s => s.SetProperty(p => p.SoftwareId, _ => twinId));

        // ====================================================================
        // 6. MagazinesBySoftware — own Id, dedup by MagazineId.
        // ====================================================================
        await context.MagazinesBySoftware
                     .Where(m => m.SoftwareId == orphanId &&
                                 context.MagazinesBySoftware.Any(t => t.SoftwareId == twinId &&
                                                                      t.MagazineId == m.MagazineId))
                     .ExecuteDeleteAsync();

        await context.MagazinesBySoftware.Where(m => m.SoftwareId == orphanId)
                     .ExecuteUpdateAsync(s => s.SetProperty(m => m.SoftwareId, _ => twinId));

        // ====================================================================
        // 7. SoftwareBySoftwareCompilation — composite PK (SoftwareId, SoftwareCompilationId).
        // ====================================================================
        // Remaining rows have SoftwareId=orphanId pointing at twin/third-party compilations.
        await context.SoftwareBySoftwareCompilation
                     .Where(j => j.SoftwareId == orphanId &&
                                 context.SoftwareBySoftwareCompilation.Any(t => t.SoftwareId == twinId &&
                                                                                t.SoftwareCompilationId == j.SoftwareCompilationId))
                     .ExecuteDeleteAsync();

        await context.SoftwareBySoftwareCompilation.Where(j => j.SoftwareId == orphanId)
                     .ExecuteUpdateAsync(s => s.SetProperty(j => j.SoftwareId, _ => twinId));

        // ====================================================================
        // 8. SoftwareVersions — dedup by VersionString, redirect rest.
        // ====================================================================
        await context.SoftwareVersions
                     .Where(v => v.SoftwareId == orphanId &&
                                 context.SoftwareVersions.Any(t => t.SoftwareId    == twinId &&
                                                                   t.VersionString == v.VersionString))
                     .ExecuteDeleteAsync();

        await context.SoftwareVersions.Where(v => v.SoftwareId == orphanId)
                     .ExecuteUpdateAsync(s => s.SetProperty(v => v.SoftwareId, _ => twinId));

        // ====================================================================
        // 9. Other one-to-many child tables (screenshots, promo art, videos,
        //    critic reviews, user ratings/reviews). No reliable dedup key — each
        //    is its own content row — so just redirect orphan's to twin.
        // ====================================================================
        await context.SoftwareScreenshots.Where(s => s.SoftwareId == orphanId)
                     .ExecuteUpdateAsync(s => s.SetProperty(x => x.SoftwareId, _ => twinId));

        await context.SoftwarePromoArt.Where(p => p.SoftwareId == orphanId)
                     .ExecuteUpdateAsync(s => s.SetProperty(p => p.SoftwareId, _ => twinId));

        await context.SoftwareVideos.Where(v => v.SoftwareId == orphanId)
                     .ExecuteUpdateAsync(s => s.SetProperty(v => v.SoftwareId, _ => twinId));

        await context.SoftwareCriticReviews.Where(r => r.SoftwareId == orphanId)
                     .ExecuteUpdateAsync(s => s.SetProperty(r => r.SoftwareId, _ => twinId));

        await context.SoftwareUserRatings.Where(r => r.SoftwareId == orphanId)
                     .ExecuteUpdateAsync(s => s.SetProperty(r => r.SoftwareId, _ => twinId));

        await context.SoftwareUserReviews.Where(r => r.SoftwareId == orphanId)
                     .ExecuteUpdateAsync(s => s.SetProperty(r => r.SoftwareId, _ => twinId));

        // ---- MobyGames-side state/download tables: 1 row per SoftwareId; dedup then redirect. ----
        // If the twin already has a row, delete the orphan's (would violate unique constraint);
        // otherwise redirect the orphan's row to the twin.

        if(await context.MobyGamesReviewImportStates.AnyAsync(r => r.SoftwareId == twinId))
            await context.MobyGamesReviewImportStates.Where(r => r.SoftwareId == orphanId).ExecuteDeleteAsync();
        else
            await context.MobyGamesReviewImportStates.Where(r => r.SoftwareId == orphanId)
                         .ExecuteUpdateAsync(s => s.SetProperty(r => r.SoftwareId, _ => twinId));

        if(await context.MobyGamesVideoImportStates.AnyAsync(r => r.SoftwareId == twinId))
            await context.MobyGamesVideoImportStates.Where(r => r.SoftwareId == orphanId).ExecuteDeleteAsync();
        else
            await context.MobyGamesVideoImportStates.Where(r => r.SoftwareId == orphanId)
                         .ExecuteUpdateAsync(s => s.SetProperty(r => r.SoftwareId, _ => twinId));

        if(await context.MobyGamesScreenshotDownloadStates.AnyAsync(r => r.SoftwareId == twinId))
            await context.MobyGamesScreenshotDownloadStates.Where(r => r.SoftwareId == orphanId).ExecuteDeleteAsync();
        else
            await context.MobyGamesScreenshotDownloadStates.Where(r => r.SoftwareId == orphanId)
                         .ExecuteUpdateAsync(s => s.SetProperty(r => r.SoftwareId, _ => twinId));

        if(await context.MobyGamesCoverDownloadStates.AnyAsync(r => r.SoftwareId == twinId))
            await context.MobyGamesCoverDownloadStates.Where(r => r.SoftwareId == orphanId).ExecuteDeleteAsync();
        else
            await context.MobyGamesCoverDownloadStates.Where(r => r.SoftwareId == orphanId)
                         .ExecuteUpdateAsync(s => s.SetProperty(r => r.SoftwareId, _ => twinId));

        if(await context.MobyGamesPromoArtDownloadStates.AnyAsync(r => r.SoftwareId == twinId))
            await context.MobyGamesPromoArtDownloadStates.Where(r => r.SoftwareId == orphanId).ExecuteDeleteAsync();
        else
            await context.MobyGamesPromoArtDownloadStates.Where(r => r.SoftwareId == orphanId)
                         .ExecuteUpdateAsync(s => s.SetProperty(r => r.SoftwareId, _ => twinId));

        // ---- Self-references on Software: redirect addons and successors. ----
        await context.Softwares.Where(s => s.BaseSoftwareId == orphanId)
                     .ExecuteUpdateAsync(s => s.SetProperty(x => x.BaseSoftwareId, _ => (ulong?)twinId));

        await context.Softwares.Where(s => s.PredecessorId == orphanId)
                     .ExecuteUpdateAsync(s => s.SetProperty(x => x.PredecessorId, _ => (ulong?)twinId));

        // ---- Finally remove the orphan Software row itself. ----
        Software orphan = await context.Softwares.FindAsync(orphanId);

        if(orphan != null)
        {
            context.Softwares.Remove(orphan);
            await context.SaveChangesAsync();
        }
    }
}
