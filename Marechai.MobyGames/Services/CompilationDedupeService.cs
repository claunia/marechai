using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.MobyGames.Services;

/// <summary>
///     Removes duplicate <see cref="SoftwareCompilation" /> rows created by re-importing the same
///     MobyGames compilation slug (e.g. <c>import-dlc-relations</c> re-importing a Season Pass once
///     per contained DLC, before compilation imports were idempotent). Every re-import overwrote the
///     state row's <see cref="MobyGamesImportState.SoftwareCompilationId" />, so the copy referenced by
///     a state row is canonical and same-name, unreferenced copies are duplicates.
///     <para>
///         A candidate is only treated as a duplicate when its contained-software set is equal to, or a
///         subset of, the canonical compilation's, so genuinely different compilations that share a
///         name (e.g. several "10 Great Games") are left alone. User data hanging off a duplicate's
///         releases (collections, standalone files, covers) is moved onto the matching canonical
///         release; when no match exists the duplicate is skipped and reported.
///     </para>
/// </summary>
public class CompilationDedupeService
{
    readonly IDbContextFactory<MarechaiContext> _contextFactory;

    public CompilationDedupeService(IDbContextFactory<MarechaiContext> contextFactory) =>
        _contextFactory = contextFactory;

    public async Task RunAsync(bool dryRun)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        Console.WriteLine("  Scanning for duplicate compilations of a state-linked twin...");

        var referencedIds = (await context.MobyGamesImportStates
                                          .Where(s => s.SoftwareCompilationId != null)
                                          .Select(s => s.SoftwareCompilationId.Value)
                                          .Distinct()
                                          .ToListAsync()).ToHashSet();

        var compilations = await context.SoftwareCompilations
                                        .Select(c => new { c.Id, c.Name })
                                        .ToListAsync();

        var groups = compilations.GroupBy(c => c.Name, StringComparer.Ordinal)
                                 .Where(g => g.Any(c => referencedIds.Contains(c.Id)) &&
                                             g.Any(c => !referencedIds.Contains(c.Id)))
                                 .OrderBy(g => g.Key, StringComparer.Ordinal)
                                 .ToList();

        if(groups.Count == 0)
        {
            Console.WriteLine("  No duplicate compilations found.");

            return;
        }

        int deleted = 0, skipped = 0;

        foreach(var group in groups)
        {
            List<ulong> canonicalIds = group.Where(c => referencedIds.Contains(c.Id)).Select(c => c.Id).ToList();
            List<ulong> candidateIds = group.Where(c => !referencedIds.Contains(c.Id)).Select(c => c.Id).ToList();

            var containedById = new Dictionary<ulong, HashSet<ulong>>();

            foreach(ulong id in canonicalIds.Concat(candidateIds))
            {
                containedById[id] = (await context.SoftwareBySoftwareCompilation
                                                  .Where(j => j.SoftwareCompilationId == id)
                                                  .Select(j => j.SoftwareId)
                                                  .ToListAsync()).ToHashSet();
            }

            Console.WriteLine($"  \"{group.Key}\": keep #{string.Join(", #", canonicalIds)}");

            foreach(ulong duplicateId in candidateIds)
            {
                HashSet<ulong> duplicateContained = containedById[duplicateId];

                ulong? canonicalId = canonicalIds.Cast<ulong?>()
                                                 .FirstOrDefault(c => duplicateContained.IsSubsetOf(containedById[c!.Value]));

                if(canonicalId is null)
                {
                    Console.WriteLine($"    #{duplicateId}: contents differ from every kept copy, left alone.");
                    skipped++;

                    continue;
                }

                if(dryRun)
                {
                    int releaseCount = await context.SoftwareReleases.CountAsync(r => r.SoftwareCompilationId == duplicateId);
                    Console.WriteLine($"    #{duplicateId}: would delete ({releaseCount} release(s)), merging into #{canonicalId}");
                    deleted++;

                    continue;
                }

                string problem = await MergeAndDeleteAsync(context, duplicateId, canonicalId.Value);

                if(problem is null)
                {
                    deleted++;
                }
                else
                {
                    Console.WriteLine($"    #{duplicateId}: skipped, {problem}");
                    skipped++;
                }
            }
        }

        Console.WriteLine(dryRun
                              ? $"\nDry run: {deleted} duplicate(s) would be deleted, {skipped} left alone"
                              : $"\nDone: {deleted} duplicate(s) deleted, {skipped} left alone");
    }

    /// <summary>
    ///     Moves anything worth keeping from <paramref name="duplicateId" /> to <paramref name="canonicalId" />
    ///     and deletes the duplicate with its releases, all in one transaction.
    /// </summary>
    /// <returns><c>null</c> on success, or why the duplicate was skipped (the transaction is rolled back).</returns>
    static async Task<string> MergeAndDeleteAsync(MarechaiContext context, ulong duplicateId, ulong canonicalId)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        List<SoftwareRelease> duplicateReleases = await context.SoftwareReleases
                                                               .Where(r => r.SoftwareCompilationId == duplicateId)
                                                               .ToListAsync();

        List<SoftwareRelease> canonicalReleases = await context.SoftwareReleases
                                                               .AsNoTracking()
                                                               .Where(r => r.SoftwareCompilationId == canonicalId)
                                                               .ToListAsync();

        bool canonicalHasCompilationCovers =
            await context.SoftwareCovers.AnyAsync(c => c.SoftwareCompilationId == canonicalId);

        int movedCollections = 0, movedFiles = 0, movedCovers = 0, droppedCovers = 0;

        foreach(SoftwareRelease release in duplicateReleases)
        {
            SoftwareRelease match = FindMatchingRelease(release, canonicalReleases);

            List<CollectedSoftwareRelease> collected = await context.CollectedSoftwareReleases
                                                                    .Where(c => c.SoftwareReleaseId == release.Id)
                                                                    .ToListAsync();

            bool hasFiles = await context.StandaloneFiles.AnyAsync(f => EF.Property<ulong>(f, "SoftwareReleaseId") == release.Id);

            if(match is null && (collected.Count > 0 || hasFiles))
            {
                await transaction.RollbackAsync();
                context.ChangeTracker.Clear();

                return $"release #{release.Id} has collections or files but no matching release in #{canonicalId}";
            }

            // CollectedSoftwareRelease is keyed on (UserId, SoftwareReleaseId), so re-key by replacing
            // the row; a user who already collected the matching release just loses the duplicate.
            foreach(CollectedSoftwareRelease entry in collected)
            {
                bool alreadyCollected = await context.CollectedSoftwareReleases
                                                     .AnyAsync(c => c.UserId == entry.UserId &&
                                                                    c.SoftwareReleaseId == match!.Id);

                if(!alreadyCollected)
                {
                    context.CollectedSoftwareReleases.Add(new CollectedSoftwareRelease
                    {
                        UserId            = entry.UserId,
                        SoftwareReleaseId = match!.Id,
                        CreatedOn         = entry.CreatedOn
                    });

                    movedCollections++;
                }

                context.CollectedSoftwareReleases.Remove(entry);
            }

            if(hasFiles)
            {
                movedFiles += await context.StandaloneFiles
                                           .Where(f => EF.Property<ulong>(f, "SoftwareReleaseId") == release.Id)
                                           .ExecuteUpdateAsync(u => u.SetProperty(f => EF.Property<ulong>(f, "SoftwareReleaseId"),
                                                                                  match!.Id));
            }

            // Release covers would be orphaned by the FK's SET NULL. Keep them on the matching release
            // (or the canonical compilation) unless the target already has covers of its own.
            List<SoftwareCover> covers = await context.SoftwareCovers
                                                      .Where(c => c.SoftwareReleaseId == release.Id)
                                                      .ToListAsync();

            if(covers.Count > 0)
            {
                bool targetHasCovers = match is not null
                                           ? await context.SoftwareCovers.AnyAsync(c => c.SoftwareReleaseId == match.Id)
                                           : canonicalHasCompilationCovers;

                foreach(SoftwareCover cover in covers)
                {
                    if(targetHasCovers)
                    {
                        context.SoftwareCovers.Remove(cover);
                        droppedCovers++;

                        continue;
                    }

                    if(match is not null)
                    {
                        cover.SoftwareReleaseId = match.Id;
                    }
                    else
                    {
                        cover.SoftwareReleaseId     = null;
                        cover.SoftwareCompilationId = canonicalId;
                    }

                    movedCovers++;
                }
            }

            await context.MobyGamesCoverDownloadStates
                         .Where(s => s.SoftwareReleaseId == release.Id)
                         .ExecuteUpdateAsync(u => u.SetProperty(s => s.SoftwareReleaseId, match != null ? match.Id : null));
        }

        // Compilation-level covers cascade on delete; keep them if the canonical copy has none.
        List<SoftwareCover> compilationCovers = await context.SoftwareCovers
                                                             .Where(c => c.SoftwareCompilationId == duplicateId)
                                                             .ToListAsync();

        foreach(SoftwareCover cover in compilationCovers)
        {
            if(canonicalHasCompilationCovers)
            {
                context.SoftwareCovers.Remove(cover);
                droppedCovers++;
            }
            else
            {
                cover.SoftwareCompilationId = canonicalId;
                movedCovers++;
            }
        }

        // Successors of the duplicate follow the canonical copy instead.
        await context.SoftwareCompilations
                     .Where(c => c.PredecessorId == duplicateId)
                     .ExecuteUpdateAsync(u => u.SetProperty(c => c.PredecessorId, canonicalId));

        // Nesting links on either side (one FK is RESTRICT): re-home them onto the canonical copy.
        List<SoftwareCompilationBySoftwareCompilation> nestingLinks =
            await context.SoftwareCompilationBySoftwareCompilation
                         .Where(j => j.ChildCompilationId == duplicateId || j.ParentCompilationId == duplicateId)
                         .ToListAsync();

        foreach(SoftwareCompilationBySoftwareCompilation link in nestingLinks)
        {
            ulong parentId = link.ParentCompilationId == duplicateId ? canonicalId : link.ParentCompilationId;
            ulong childId  = link.ChildCompilationId  == duplicateId ? canonicalId : link.ChildCompilationId;

            bool exists = await context.SoftwareCompilationBySoftwareCompilation
                                       .AnyAsync(j => j.ParentCompilationId == parentId &&
                                                      j.ChildCompilationId  == childId);

            if(!exists && parentId != childId)
            {
                context.SoftwareCompilationBySoftwareCompilation.Add(new SoftwareCompilationBySoftwareCompilation
                {
                    ParentCompilationId = parentId,
                    ChildCompilationId  = childId
                });
            }

            context.SoftwareCompilationBySoftwareCompilation.Remove(link);
        }

        SoftwareCompilation duplicate = await context.SoftwareCompilations.FirstAsync(c => c.Id == duplicateId);

        if(duplicate.BaseSoftwareId is not null)
        {
            await context.SoftwareCompilations
                         .Where(c => c.Id == canonicalId && c.BaseSoftwareId == null)
                         .ExecuteUpdateAsync(u => u.SetProperty(c => c.BaseSoftwareId, duplicate.BaseSoftwareId));
        }

        await context.SaveChangesAsync();

        // Tracked removals so the search-index interceptor drops the duplicate's entries.
        context.SoftwareReleases.RemoveRange(duplicateReleases);
        await context.SaveChangesAsync();

        context.SoftwareCompilations.Remove(duplicate);
        await context.SaveChangesAsync();

        await transaction.CommitAsync();
        context.ChangeTracker.Clear();

        Console.WriteLine($"    #{duplicateId}: deleted ({duplicateReleases.Count} release(s)); moved " +
                          $"{movedCollections} collection entr(ies), {movedFiles} file(s), {movedCovers} cover(s); " +
                          $"dropped {droppedCovers} redundant cover(s)");

        return null;
    }

    /// <summary>
    ///     Finds the canonical release that corresponds to a duplicate one: same platform, title,
    ///     publisher and date first, then same platform and title.
    /// </summary>
    static SoftwareRelease FindMatchingRelease(SoftwareRelease release, List<SoftwareRelease> candidates) =>
        candidates.FirstOrDefault(c => c.PlatformId  == release.PlatformId  &&
                                       c.Title       == release.Title       &&
                                       c.PublisherId == release.PublisherId &&
                                       c.ReleaseDate == release.ReleaseDate) ??
        candidates.FirstOrDefault(c => c.PlatformId == release.PlatformId && c.Title == release.Title);
}
