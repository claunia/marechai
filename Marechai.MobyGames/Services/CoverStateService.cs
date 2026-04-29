using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.MobyGames.Services;

public class CoverStateService
{
    readonly IDbContextFactory<MarechaiContext> _contextFactory;

    public CoverStateService(IDbContextFactory<MarechaiContext> contextFactory) => _contextFactory = contextFactory;

    public async Task<HashSet<string>> GetProcessedCoverUrlsAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var urls = await context.MobyGamesCoverDownloadStates
                                .Where(s => s.Status == MobyGamesCoverDownloadStatus.Downloaded)
                                .Select(s => s.CoverPageUrl)
                                .ToListAsync();

        return [..urls];
    }

    public async Task<List<MobyGamesCoverDownloadState>> GetStatesForGameAsync(string mobyGameId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        return await context.MobyGamesCoverDownloadStates
                            .Where(s => s.MobyGameId == mobyGameId)
                            .ToListAsync();
    }

    public async Task<MobyGamesCoverDownloadState> GetStateByCoverUrlAsync(string coverPageUrl)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        return await context.MobyGamesCoverDownloadStates
                            .FirstOrDefaultAsync(s => s.CoverPageUrl == coverPageUrl);
    }

    public async Task CreateStateAsync(MobyGamesCoverDownloadState state)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        context.MobyGamesCoverDownloadStates.Add(state);
        await context.SaveChangesAsync();
    }

    public async Task UpdateStateAsync(MobyGamesCoverDownloadState state)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var existing = await context.MobyGamesCoverDownloadStates
                                    .FirstOrDefaultAsync(s => s.Id == state.Id);

        if(existing is null) return;

        existing.Status            = state.Status;
        existing.ErrorMessage      = state.ErrorMessage;
        existing.ProcessedOn       = state.ProcessedOn;
        existing.SoftwareCoverId   = state.SoftwareCoverId;
        existing.SoftwareReleaseId = state.SoftwareReleaseId;
        existing.OriginalUrl       = state.OriginalUrl;

        await context.SaveChangesAsync();
    }

    public async Task PrintCoverStatusAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        int pending    = await context.MobyGamesCoverDownloadStates.CountAsync(s => s.Status == MobyGamesCoverDownloadStatus.Pending);
        int downloaded = await context.MobyGamesCoverDownloadStates.CountAsync(s => s.Status == MobyGamesCoverDownloadStatus.Downloaded);
        int failed     = await context.MobyGamesCoverDownloadStates.CountAsync(s => s.Status == MobyGamesCoverDownloadStatus.Failed);
        int skipped    = await context.MobyGamesCoverDownloadStates.CountAsync(s => s.Status == MobyGamesCoverDownloadStatus.Skipped);
        int noRelease  = await context.MobyGamesCoverDownloadStates.CountAsync(s => s.Status == MobyGamesCoverDownloadStatus.NoRelease);
        int total      = pending + downloaded + failed + skipped + noRelease;

        Console.WriteLine($"\n  Cover Download Status:");
        Console.WriteLine($"    Downloaded: {downloaded}");
        Console.WriteLine($"    Failed:     {failed}");
        Console.WriteLine($"    Skipped:    {skipped}");
        Console.WriteLine($"    No Release: {noRelease}");
        Console.WriteLine($"    Pending:    {pending}");
        Console.WriteLine($"    Total:      {total}");
    }
}
