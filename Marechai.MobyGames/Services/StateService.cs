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

    public async Task MarkImportedAsync(string mobyGameId, int batchNumber, ulong? softwareId,
                                        int? mobyNumericId = null)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var existing = await context.MobyGamesImportStates
                                    .FirstOrDefaultAsync(s => s.MobyGameId == mobyGameId);

        if(existing != null)
        {
            existing.Status        = MobyGamesImportStatus.Imported;
            existing.ProcessedOn   = DateTime.UtcNow;
            existing.BatchNumber   = batchNumber;
            existing.SoftwareId    = softwareId;
            existing.MobyNumericId = mobyNumericId ?? existing.MobyNumericId;
        }
        else
        {
            context.MobyGamesImportStates.Add(new MobyGamesImportState
            {
                MobyGameId     = mobyGameId,
                Status         = MobyGamesImportStatus.Imported,
                ProcessedOn    = DateTime.UtcNow,
                BatchNumber    = batchNumber,
                SoftwareId     = softwareId,
                MobyNumericId  = mobyNumericId
            });
        }

        await context.SaveChangesAsync();
    }

    public async Task MarkRejectedAsync(string mobyGameId, string gameName, string reason, int batchNumber)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var existing = await context.MobyGamesImportStates
                                    .FirstOrDefaultAsync(s => s.MobyGameId == mobyGameId);

        if(existing != null)
        {
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

    public async Task MarkFailedAsync(string mobyGameId, string error, int batchNumber)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var existing = await context.MobyGamesImportStates
                                    .FirstOrDefaultAsync(s => s.MobyGameId == mobyGameId);

        if(existing != null)
        {
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
