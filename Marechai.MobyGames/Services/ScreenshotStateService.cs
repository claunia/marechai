using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.MobyGames.Services;

public class ScreenshotStateService
{
    readonly IDbContextFactory<MarechaiContext> _contextFactory;

    public ScreenshotStateService(IDbContextFactory<MarechaiContext> contextFactory) =>
        _contextFactory = contextFactory;

    public async Task<HashSet<string>> GetProcessedScreenshotUrlsAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var urls = await context.MobyGamesScreenshotDownloadStates
                                .Where(s => s.Status == MobyGamesCoverDownloadStatus.Downloaded)
                                .Select(s => s.ScreenshotPageUrl)
                                .ToListAsync();

        return [..urls];
    }

    public async Task<MobyGamesScreenshotDownloadState> GetStateByScreenshotUrlAsync(string screenshotPageUrl)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        return await context.MobyGamesScreenshotDownloadStates
                            .FirstOrDefaultAsync(s => s.ScreenshotPageUrl == screenshotPageUrl);
    }

    public async Task CreateStateAsync(MobyGamesScreenshotDownloadState state)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        context.MobyGamesScreenshotDownloadStates.Add(state);
        await context.SaveChangesAsync();
    }

    public async Task UpdateStateAsync(MobyGamesScreenshotDownloadState state)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var existing = await context.MobyGamesScreenshotDownloadStates
                                    .FirstOrDefaultAsync(s => s.Id == state.Id);

        if(existing is null) return;

        existing.Status               = state.Status;
        existing.ErrorMessage         = state.ErrorMessage;
        existing.ProcessedOn          = state.ProcessedOn;
        existing.SoftwareScreenshotId = state.SoftwareScreenshotId;
        existing.OriginalUrl          = state.OriginalUrl;

        await context.SaveChangesAsync();
    }

    public async Task PrintScreenshotStatusAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        int pending    = await context.MobyGamesScreenshotDownloadStates.CountAsync(s => s.Status == MobyGamesCoverDownloadStatus.Pending);
        int downloaded = await context.MobyGamesScreenshotDownloadStates.CountAsync(s => s.Status == MobyGamesCoverDownloadStatus.Downloaded);
        int failed     = await context.MobyGamesScreenshotDownloadStates.CountAsync(s => s.Status == MobyGamesCoverDownloadStatus.Failed);
        int skipped    = await context.MobyGamesScreenshotDownloadStates.CountAsync(s => s.Status == MobyGamesCoverDownloadStatus.Skipped);

        Console.WriteLine("\n  Screenshot Download Status:");
        Console.WriteLine($"    Pending:    {pending}");
        Console.WriteLine($"    Downloaded: {downloaded}");
        Console.WriteLine($"    Failed:     {failed}");
        Console.WriteLine($"    Skipped:    {skipped}");
        Console.WriteLine($"    Total:      {pending + downloaded + failed + skipped}\n");
    }
}
