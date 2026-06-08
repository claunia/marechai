using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.MobyGames.Services;

public class PromoArtStateService
{
    readonly IDbContextFactory<MarechaiContext> _contextFactory;

    public PromoArtStateService(IDbContextFactory<MarechaiContext> contextFactory) => _contextFactory = contextFactory;

    public async Task<HashSet<string>> GetProcessedPromoUrlsAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var urls = await context.MobyGamesPromoArtDownloadStates
                                .Where(s => s.Status == MobyGamesCoverDownloadStatus.Downloaded)
                                .Select(s => s.PromoPageUrl)
                                .ToListAsync();

        return [..urls];
    }

    public async Task<MobyGamesPromoArtDownloadState> GetStateByPromoUrlAsync(string promoPageUrl)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        return await context.MobyGamesPromoArtDownloadStates
                            .FirstOrDefaultAsync(s => s.PromoPageUrl == promoPageUrl);
    }

    public async Task CreateStateAsync(MobyGamesPromoArtDownloadState state)
    {
        if(state.Caption?.Length > 256)
            state.Caption = state.Caption[..256];

        await using var context = await _contextFactory.CreateDbContextAsync();

        context.MobyGamesPromoArtDownloadStates.Add(state);
        await context.SaveChangesAsync();
    }

    public async Task UpdateStateAsync(MobyGamesPromoArtDownloadState state)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var existing = await context.MobyGamesPromoArtDownloadStates
                                    .FirstOrDefaultAsync(s => s.Id == state.Id);

        if(existing is null) return;

        existing.Status             = state.Status;
        existing.ErrorMessage       = state.ErrorMessage;
        existing.ProcessedOn        = state.ProcessedOn;
        existing.SoftwarePromoArtId = state.SoftwarePromoArtId;
        existing.OriginalUrl        = state.OriginalUrl;

        await context.SaveChangesAsync();
    }

    public async Task PrintPromoArtStatusAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        int pending    = await context.MobyGamesPromoArtDownloadStates.CountAsync(s => s.Status == MobyGamesCoverDownloadStatus.Pending);
        int downloaded = await context.MobyGamesPromoArtDownloadStates.CountAsync(s => s.Status == MobyGamesCoverDownloadStatus.Downloaded);
        int failed     = await context.MobyGamesPromoArtDownloadStates.CountAsync(s => s.Status == MobyGamesCoverDownloadStatus.Failed);
        int skipped    = await context.MobyGamesPromoArtDownloadStates.CountAsync(s => s.Status == MobyGamesCoverDownloadStatus.Skipped);

        Console.WriteLine("\n  Promo Art Download Status:");
        Console.WriteLine($"    Pending:    {pending}");
        Console.WriteLine($"    Downloaded: {downloaded}");
        Console.WriteLine($"    Failed:     {failed}");
        Console.WriteLine($"    Skipped:    {skipped}");
        Console.WriteLine($"    Total:      {pending + downloaded + failed + skipped}\n");
    }
}
