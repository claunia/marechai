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
    ///     delete the orphan's row (if it would violate a unique constraint). Finally deletes
    ///     the orphan <see cref="Software" /> row itself.
    /// </summary>
    async Task MergeOneAsync(ulong orphanId, ulong twinId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        // ---- Tables with multi-column uniqueness: dedup first, then redirect remainder. ----

        // SoftwareDescription: unique per (SoftwareId, LanguageCode)
        var twinDescSet = new HashSet<string>(
            await context.SoftwareDescriptions
                         .Where(d => d.SoftwareId == twinId)
                         .Select(d => d.LanguageCode)
                         .ToListAsync());

        foreach(var d in await context.SoftwareDescriptions.Where(x => x.SoftwareId == orphanId).ToListAsync())
        {
            if(twinDescSet.Contains(d.LanguageCode))
                context.SoftwareDescriptions.Remove(d);
            else
                d.SoftwareId = twinId;
        }

        // GenreBySoftware: unique per (SoftwareId, GenreId)
        var twinGenreSet = new HashSet<int>(
            await context.GenresBySoftware
                         .Where(g => g.SoftwareId == twinId)
                         .Select(g => g.GenreId)
                         .ToListAsync());

        foreach(var g in await context.GenresBySoftware.Where(x => x.SoftwareId == orphanId).ToListAsync())
        {
            if(twinGenreSet.Contains(g.GenreId))
                context.GenresBySoftware.Remove(g);
            else
                g.SoftwareId = twinId;
        }

        // SoftwareCompanyRole: dedup by (CompanyId, RoleId)
        var twinCompanyRoleSet = new HashSet<(int, string)>(
            (await context.SoftwareCompanyRoles
                          .Where(r => r.SoftwareId == twinId)
                          .Select(r => new { r.CompanyId, r.RoleId })
                          .ToListAsync())
            .Select(x => (x.CompanyId, x.RoleId)));

        foreach(var r in await context.SoftwareCompanyRoles.Where(x => x.SoftwareId == orphanId).ToListAsync())
        {
            if(twinCompanyRoleSet.Contains((r.CompanyId, r.RoleId)))
                context.SoftwareCompanyRoles.Remove(r);
            else
                r.SoftwareId = twinId;
        }

        // PeopleBySoftware has an autoincrement Id (BaseModel<long>) — no implicit uniqueness on
        // (SoftwareId, PersonId, RoleId), but dedup conservatively to avoid duplicate credits.
        var twinPeopleSet = new HashSet<(int, string)>(
            (await context.PeopleBySoftware
                          .Where(p => p.SoftwareId == twinId)
                          .Select(p => new { p.PersonId, p.RoleId })
                          .ToListAsync())
            .Select(x => (x.PersonId, x.RoleId)));

        foreach(var p in await context.PeopleBySoftware.Where(x => x.SoftwareId == orphanId).ToListAsync())
        {
            if(twinPeopleSet.Contains((p.PersonId, p.RoleId)))
                context.PeopleBySoftware.Remove(p);
            else
                p.SoftwareId = twinId;
        }

        // MagazinesBySoftware: dedup by MagazineId
        var twinMagSet = new HashSet<long>(
            await context.MagazinesBySoftware
                         .Where(m => m.SoftwareId == twinId)
                         .Select(m => m.MagazineId)
                         .ToListAsync());

        foreach(var m in await context.MagazinesBySoftware.Where(x => x.SoftwareId == orphanId).ToListAsync())
        {
            if(twinMagSet.Contains(m.MagazineId))
                context.MagazinesBySoftware.Remove(m);
            else
                m.SoftwareId = twinId;
        }

        // SoftwareBySoftwareRelease: dedup by ReleaseId
        var twinSwSrSet = new HashSet<ulong>(
            await context.SoftwareBySoftwareRelease
                         .Where(j => j.SoftwareId == twinId)
                         .Select(j => j.ReleaseId)
                         .ToListAsync());

        foreach(var j in await context.SoftwareBySoftwareRelease.Where(x => x.SoftwareId == orphanId).ToListAsync())
        {
            if(twinSwSrSet.Contains(j.ReleaseId))
                context.SoftwareBySoftwareRelease.Remove(j);
            else
                j.SoftwareId = twinId;
        }

        await context.SaveChangesAsync();

        // ---- Tables without a convenient dedup key: bulk-redirect via ExecuteUpdate. ----

        await context.SoftwareReleases.Where(r => r.SoftwareId == orphanId)
                     .ExecuteUpdateAsync(s => s.SetProperty(r => r.SoftwareId, _ => (ulong?)twinId));

        await context.SoftwareVersions.Where(v => v.SoftwareId == orphanId)
                     .ExecuteUpdateAsync(s => s.SetProperty(v => v.SoftwareId, _ => twinId));

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
