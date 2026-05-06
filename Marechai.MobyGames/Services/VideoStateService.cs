using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.MobyGames.Services;

public class VideoStateService
{
    readonly IDbContextFactory<MarechaiContext> _contextFactory;

    public VideoStateService(IDbContextFactory<MarechaiContext> contextFactory) =>
        _contextFactory = contextFactory;

    public async Task<HashSet<string>> GetProcessedVideoUrlsAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var urls = await context.MobyGamesVideoImportStates
                                .Where(s => s.Status == MobyGamesCoverDownloadStatus.Downloaded)
                                .Select(s => s.VideoUrl)
                                .ToListAsync();

        return [..urls];
    }

    public async Task<MobyGamesVideoImportState> GetStateByVideoUrlAsync(string videoUrl)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        return await context.MobyGamesVideoImportStates
                            .FirstOrDefaultAsync(s => s.VideoUrl == videoUrl);
    }

    public async Task CreateStateAsync(MobyGamesVideoImportState state)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        context.MobyGamesVideoImportStates.Add(state);
        await context.SaveChangesAsync();
    }

    public async Task UpdateStateAsync(MobyGamesVideoImportState state)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var existing = await context.MobyGamesVideoImportStates
                                    .FirstOrDefaultAsync(s => s.Id == state.Id);

        if(existing is null) return;

        existing.Status          = state.Status;
        existing.ErrorMessage    = state.ErrorMessage;
        existing.ProcessedOn     = state.ProcessedOn;
        existing.SoftwareVideoId = state.SoftwareVideoId;

        await context.SaveChangesAsync();
    }

    public async Task PrintVideoStatusAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        int pending    = await context.MobyGamesVideoImportStates.CountAsync(s => s.Status == MobyGamesCoverDownloadStatus.Pending);
        int downloaded = await context.MobyGamesVideoImportStates.CountAsync(s => s.Status == MobyGamesCoverDownloadStatus.Downloaded);
        int failed     = await context.MobyGamesVideoImportStates.CountAsync(s => s.Status == MobyGamesCoverDownloadStatus.Failed);
        int skipped    = await context.MobyGamesVideoImportStates.CountAsync(s => s.Status == MobyGamesCoverDownloadStatus.Skipped);

        Console.WriteLine("\n  Video Import Status:");
        Console.WriteLine($"    Pending:    {pending}");
        Console.WriteLine($"    Imported:   {downloaded}");
        Console.WriteLine($"    Failed:     {failed}");
        Console.WriteLine($"    Skipped:    {skipped}");
        Console.WriteLine($"    Total:      {pending + downloaded + failed + skipped}\n");
    }
}
