using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.MobyGames.Services;

public class StateService
{
    readonly IDbContextFactory<MarechaiContext> _contextFactory;

    public StateService(IDbContextFactory<MarechaiContext> contextFactory) => _contextFactory = contextFactory;

    public async Task<HashSet<string>> GetProcessedGameIdsAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var ids = await context.MobyGamesImportStates
                               .Select(s => s.MobyGameId)
                               .ToListAsync();

        return [..ids];
    }

    /// <summary>
    ///     Records that a <see cref="Software" /> row has been created or linked for the given
    ///     MobyGames slug. Must be called immediately after the <see cref="Software" /> row is
    ///     committed (the SAME <c>SaveChangesAsync</c> that committed the Software cannot also
    ///     write this row because <see cref="Software" /> creation and the state-row upsert use
    ///     a fresh <see cref="MarechaiContext" /> here for safety against concurrent state writes
    ///     from sibling services).
    ///     <para>
    ///         Maintains the invariant "every importer-created Software has a state row holding
    ///         its slug and SoftwareId". <see cref="MarkFailedAsync" /> and
    ///         <see cref="MarkRejectedAsync" /> preserve the <see cref="MobyGamesImportState.SoftwareId" />
    ///         set here so a mid-import failure cannot leave the Software dangling.
    ///     </para>
    /// </summary>
    public async Task MarkSoftwareLinkedAsync(string mobyGameId, ulong softwareId, int batchNumber,
                                              int? mobyNumericId = null)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var existing = await context.MobyGamesImportStates
                                    .FirstOrDefaultAsync(s => s.MobyGameId == mobyGameId);

        if(existing != null)
        {
            existing.SoftwareId    = softwareId;
            existing.BatchNumber   = batchNumber;
            existing.ErrorMessage  = null;
            existing.MobyNumericId = mobyNumericId ?? existing.MobyNumericId;

            // Defensive: keep Imported as-is (the batch loop normally skips imported slugs, so
            // reaching this method with an Imported row should not happen). Otherwise the slug
            // is now in flight again — Pending until the final outcome is written.
            if(existing.Status != MobyGamesImportStatus.Imported)
                existing.Status = MobyGamesImportStatus.Pending;
        }
        else
        {
            context.MobyGamesImportStates.Add(new MobyGamesImportState
            {
                MobyGameId    = mobyGameId,
                Status        = MobyGamesImportStatus.Pending,
                BatchNumber   = batchNumber,
                SoftwareId    = softwareId,
                MobyNumericId = mobyNumericId
            });
        }

        await UpsertExternalIdAsync(context, mobyGameId, mobyNumericId ?? existing?.MobyNumericId, softwareId);

        await context.SaveChangesAsync();
    }

    /// <summary>
    ///     Mirrors the (site, id) -&gt; Software link into the generic <see cref="SoftwareExternalId" />
    ///     table alongside the MobyGames-specific state row above. Prefers MobyGames' numeric id (stable,
    ///     always resolvable to a URL); falls back to the slug when the numeric id isn't known yet. If a
    ///     row already exists under the slug and the numeric id later becomes available, it is upgraded
    ///     in place rather than left stale. Caller's context/SaveChanges is reused.
    /// </summary>
    static async Task UpsertExternalIdAsync(MarechaiContext context, string mobyGameId, int? mobyNumericId,
                                             ulong softwareId)
    {
        long? siteId = (await context.ExternalSites.FirstOrDefaultAsync(s => s.Name == "MobyGames"))?.Id;

        if(siteId is null)
            return;

        string externalId = mobyNumericId?.ToString() ?? mobyGameId;

        var existingExternalId =
            await context.SoftwareExternalIds.FirstOrDefaultAsync(e => e.ExternalSiteId == siteId.Value &&
                                                                        (e.ExternalId == externalId ||
                                                                         e.ExternalId == mobyGameId));

        if(existingExternalId != null)
        {
            existingExternalId.SoftwareId = softwareId;
            existingExternalId.ExternalId = externalId;
        }
        else
            context.SoftwareExternalIds.Add(new SoftwareExternalId
            {
                SoftwareId     = softwareId,
                ExternalSiteId = siteId.Value,
                ExternalId     = externalId
            });
    }

    public async Task MarkImportedAsync(string mobyGameId, int batchNumber, ulong? softwareId,
                                        int? mobyNumericId = null, ulong? softwareCompilationId = null)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var existing = await context.MobyGamesImportStates
                                    .FirstOrDefaultAsync(s => s.MobyGameId == mobyGameId);

        if(existing != null)
        {
            existing.Status                = MobyGamesImportStatus.Imported;
            existing.ProcessedOn           = DateTime.UtcNow;
            existing.BatchNumber           = batchNumber;
            existing.SoftwareId            = softwareId;
            existing.SoftwareCompilationId = softwareCompilationId;
            existing.MobyNumericId         = mobyNumericId ?? existing.MobyNumericId;
        }
        else
        {
            context.MobyGamesImportStates.Add(new MobyGamesImportState
            {
                MobyGameId            = mobyGameId,
                Status                = MobyGamesImportStatus.Imported,
                ProcessedOn           = DateTime.UtcNow,
                BatchNumber           = batchNumber,
                SoftwareId            = softwareId,
                SoftwareCompilationId = softwareCompilationId,
                MobyNumericId         = mobyNumericId
            });
        }

        if(softwareId.HasValue)
            await UpsertExternalIdAsync(context, mobyGameId, mobyNumericId ?? existing?.MobyNumericId,
                                         softwareId.Value);

        await context.SaveChangesAsync();
    }

    /// <summary>
    ///     Marks a slug as Rejected. INVARIANT: never clears <see cref="MobyGamesImportState.SoftwareId" />
    ///     on the update branch — if a Software was already linked for this slug by
    ///     <see cref="MarkSoftwareLinkedAsync" />, that linkage must survive the status change.
    ///     The Add branch leaves <c>SoftwareId</c> null because reaching this method without an
    ///     existing row means no Software was created for this slug.
    /// </summary>
    public async Task MarkRejectedAsync(string mobyGameId, string gameName, string reason, int batchNumber)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var existing = await context.MobyGamesImportStates
                                    .FirstOrDefaultAsync(s => s.MobyGameId == mobyGameId);

        if(existing != null)
        {
            // Do NOT touch SoftwareId / MobyNumericId — preserve any link made earlier.
            existing.Status      = MobyGamesImportStatus.Rejected;
            existing.ProcessedOn = DateTime.UtcNow;
            existing.BatchNumber = batchNumber;
        }
        else
        {
            context.MobyGamesImportStates.Add(new MobyGamesImportState
            {
                MobyGameId   = mobyGameId,
                Status       = MobyGamesImportStatus.Rejected,
                ProcessedOn  = DateTime.UtcNow,
                BatchNumber  = batchNumber
            });
        }

        context.MobyGamesRejections.Add(new MobyGamesRejection
        {
            MobyGameId   = mobyGameId,
            GameName     = gameName ?? "Unknown",
            Reason       = reason,
            RejectedOn   = DateTime.UtcNow,
            ReviewAction = MobyGamesRejectionReview.Pending
        });

        await context.SaveChangesAsync();
    }

    /// <summary>
    ///     Marks a slug as Failed. INVARIANT: never clears <see cref="MobyGamesImportState.SoftwareId" />
    ///     on the update branch — if a Software was already linked for this slug by
    ///     <see cref="MarkSoftwareLinkedAsync" /> (and that Software is now committed in the DB),
    ///     the linkage must survive the failure. Otherwise the Software is left dangling and the
    ///     DLC / compilation linkers can't find their way back to the slug.
    ///     The Add branch leaves <c>SoftwareId</c> null because reaching this method without an
    ///     existing row means the failure happened before any Software was created.
    /// </summary>
    public async Task MarkFailedAsync(string mobyGameId, string error, int batchNumber)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var existing = await context.MobyGamesImportStates
                                    .FirstOrDefaultAsync(s => s.MobyGameId == mobyGameId);

        if(existing != null)
        {
            // Do NOT touch SoftwareId / MobyNumericId — preserve any link made earlier.
            existing.Status       = MobyGamesImportStatus.Failed;
            existing.ErrorMessage = error?[..Math.Min(error.Length, 1024)];
            existing.ProcessedOn  = DateTime.UtcNow;
            existing.BatchNumber  = batchNumber;
        }
        else
        {
            context.MobyGamesImportStates.Add(new MobyGamesImportState
            {
                MobyGameId   = mobyGameId,
                Status       = MobyGamesImportStatus.Failed,
                ErrorMessage = error?[..Math.Min(error?.Length ?? 0, 1024)],
                ProcessedOn  = DateTime.UtcNow,
                BatchNumber  = batchNumber
            });
        }

        await context.SaveChangesAsync();
    }

    public async Task PrintStatusAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        int pending  = await context.MobyGamesImportStates.CountAsync(s => s.Status == MobyGamesImportStatus.Pending);
        int imported = await context.MobyGamesImportStates.CountAsync(s => s.Status == MobyGamesImportStatus.Imported);
        int rejected = await context.MobyGamesImportStates.CountAsync(s => s.Status == MobyGamesImportStatus.Rejected);
        int failed   = await context.MobyGamesImportStates.CountAsync(s => s.Status == MobyGamesImportStatus.Failed);
        int total    = pending + imported + rejected + failed;

        Console.WriteLine($"\n  Import Status:");
        Console.WriteLine($"    Imported: {imported}");
        Console.WriteLine($"    Rejected: {rejected}");
        Console.WriteLine($"    Failed:   {failed}");
        Console.WriteLine($"    Pending:  {pending}");
        Console.WriteLine($"    Total:    {total}");
    }

    public async Task ResetGameAsync(string mobyGameId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var state = await context.MobyGamesImportStates
                                 .FirstOrDefaultAsync(s => s.MobyGameId == mobyGameId);

        if(state != null)
        {
            context.MobyGamesImportStates.Remove(state);
            await context.SaveChangesAsync();
            Console.WriteLine($"  Reset game {mobyGameId} to unprocessed");
        }
        else
        {
            Console.WriteLine($"  Game {mobyGameId} not found in import state");
        }
    }
}
