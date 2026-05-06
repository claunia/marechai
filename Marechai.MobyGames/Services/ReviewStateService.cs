using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.MobyGames.Services;

public class ReviewStateService
{
    readonly IDbContextFactory<MarechaiContext> _contextFactory;

    public ReviewStateService(IDbContextFactory<MarechaiContext> contextFactory) => _contextFactory = contextFactory;

    public async Task<HashSet<string>> GetProcessedGameIdsAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var ids = await context.MobyGamesReviewImportStates
                               .Where(s => s.Status == MobyGamesReviewImportStatus.Imported ||
                                           s.Status == MobyGamesReviewImportStatus.NoReviews)
                               .Select(s => s.MobyGameId)
                               .ToListAsync();

        return [..ids];
    }

    public async Task CreateOrUpdateStateAsync(string mobyGameId, ulong softwareId,
                                               MobyGamesReviewImportStatus status,
                                               int reviewsImported = 0,
                                               string errorMessage = null)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var existing = await context.MobyGamesReviewImportStates
                                    .FirstOrDefaultAsync(s => s.MobyGameId == mobyGameId);

        if(existing != null)
        {
            existing.Status          = status;
            existing.ProcessedOn     = DateTime.UtcNow;
            existing.ReviewsImported = reviewsImported;
            existing.ErrorMessage    = errorMessage;
        }
        else
        {
            context.MobyGamesReviewImportStates.Add(new MobyGamesReviewImportState
            {
                MobyGameId      = mobyGameId,
                SoftwareId      = softwareId,
                Status          = status,
                ProcessedOn     = DateTime.UtcNow,
                ReviewsImported = reviewsImported,
                ErrorMessage    = errorMessage
            });
        }

        await context.SaveChangesAsync();
    }

    public async Task PrintStatusAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        int pending   = await context.MobyGamesReviewImportStates.CountAsync(s => s.Status == MobyGamesReviewImportStatus.Pending);
        int imported  = await context.MobyGamesReviewImportStates.CountAsync(s => s.Status == MobyGamesReviewImportStatus.Imported);
        int failed    = await context.MobyGamesReviewImportStates.CountAsync(s => s.Status == MobyGamesReviewImportStatus.Failed);
        int skipped   = await context.MobyGamesReviewImportStates.CountAsync(s => s.Status == MobyGamesReviewImportStatus.Skipped);
        int noReviews = await context.MobyGamesReviewImportStates.CountAsync(s => s.Status == MobyGamesReviewImportStatus.NoReviews);
        int total     = pending + imported + failed + skipped + noReviews;

        int totalReviews = imported > 0
                               ? await context.MobyGamesReviewImportStates
                                              .Where(s => s.Status == MobyGamesReviewImportStatus.Imported)
                                              .SumAsync(s => s.ReviewsImported)
                               : 0;

        Console.WriteLine("\n  Review Import Status:");
        Console.WriteLine($"    Imported:    {imported} ({totalReviews} total reviews)");
        Console.WriteLine($"    No Reviews:  {noReviews}");
        Console.WriteLine($"    Failed:      {failed}");
        Console.WriteLine($"    Skipped:     {skipped}");
        Console.WriteLine($"    Pending:     {pending}");
        Console.WriteLine($"    Total:       {total}");
    }
}
