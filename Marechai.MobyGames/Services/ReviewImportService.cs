using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Database.Models;
using Marechai.MobyGames.Parsers;
using Microsoft.EntityFrameworkCore;

namespace Marechai.MobyGames.Services;

public class ReviewImportService
{
    readonly IDbContextFactory<MarechaiContext> _contextFactory;
    readonly SourceDatabaseService             _sourceDb;
    readonly PlatformMatcher                   _platformMatcher;
    readonly MagazineMatcher                   _magazineMatcher;
    readonly ReviewStateService                _reviewStateService;

    public ReviewImportService(IDbContextFactory<MarechaiContext> contextFactory,
                               SourceDatabaseService sourceDb,
                               PlatformMatcher platformMatcher,
                               MagazineMatcher magazineMatcher,
                               ReviewStateService reviewStateService)
    {
        _contextFactory     = contextFactory;
        _sourceDb           = sourceDb;
        _platformMatcher    = platformMatcher;
        _magazineMatcher    = magazineMatcher;
        _reviewStateService = reviewStateService;
    }

    public async Task RunAsync(int batchSize)
    {
        Console.WriteLine("  Loading reference data...");
        await _platformMatcher.LoadAsync();
        await _magazineMatcher.LoadAsync();

        // Get all imported games with SoftwareId
        await using var context = await _contextFactory.CreateDbContextAsync();

        var importedGames = await context.MobyGamesImportStates
                                         .Where(s => s.Status    == MobyGamesImportStatus.Imported &&
                                                     s.SoftwareId != null)
                                         .Select(s => new
                                         {
                                             s.MobyGameId,
                                             SoftwareId = s.SoftwareId.Value
                                         })
                                         .ToListAsync();

        Console.WriteLine($"  Total imported games: {importedGames.Count}");

        // Get already-processed review imports for resume
        var processedIds = await _reviewStateService.GetProcessedGameIdsAsync();

        var toProcess = importedGames
                       .Where(g => !processedIds.Contains(g.MobyGameId))
                       .Take(batchSize)
                       .ToList();

        Console.WriteLine($"  Games to process in this batch: {toProcess.Count}");

        int processed    = 0;
        int totalReviews = 0;

        foreach(var game in toProcess)
        {
            processed++;
            Console.Write($"\n  [{processed}/{toProcess.Count}] {game.MobyGameId} (Software ID: {game.SoftwareId})");

            try
            {
                // Get all HTML chunks for this game from the source database
                var rows = await _sourceDb.GetRowsForGameAsync(game.MobyGameId);

                if(rows.Count == 0)
                {
                    Console.WriteLine(" — no source data");

                    await _reviewStateService.CreateOrUpdateStateAsync(
                        game.MobyGameId, game.SoftwareId,
                        MobyGamesReviewImportStatus.Skipped,
                        errorMessage: "No source HTML chunks found");

                    continue;
                }

                // Find the Reviews tab chunk
                string reviewsHtml = null;

                foreach(var row in rows)
                {
                    if(TabDetector.Detect(row.Body) == MobyTab.Reviews)
                    {
                        reviewsHtml = row.Body;

                        break;
                    }
                }

                if(reviewsHtml is null)
                {
                    Console.WriteLine(" — no reviews tab");

                    await _reviewStateService.CreateOrUpdateStateAsync(
                        game.MobyGameId, game.SoftwareId,
                        MobyGamesReviewImportStatus.NoReviews);

                    continue;
                }

                // Parse critic reviews from the old-format HTML
                var reviews = ReviewsPageParser.Parse(reviewsHtml);

                if(reviews.Count == 0)
                {
                    Console.WriteLine(" — no critic reviews");

                    await _reviewStateService.CreateOrUpdateStateAsync(
                        game.MobyGameId, game.SoftwareId,
                        MobyGamesReviewImportStatus.NoReviews);

                    continue;
                }

                Console.WriteLine($" — {reviews.Count} critic reviews");

                // Import reviews
                int imported = await ImportReviewsAsync(game.SoftwareId, reviews);
                totalReviews += imported;

                await _reviewStateService.CreateOrUpdateStateAsync(
                    game.MobyGameId, game.SoftwareId,
                    MobyGamesReviewImportStatus.Imported,
                    reviewsImported: imported);

                Console.WriteLine($"    Imported {imported} reviews");
            }
            catch(Exception ex)
            {
                Console.WriteLine($" — \e[31mError: {ex.Message}\e[0m");

                await _reviewStateService.CreateOrUpdateStateAsync(
                    game.MobyGameId, game.SoftwareId,
                    MobyGamesReviewImportStatus.Failed,
                    errorMessage: ex.Message.Length > 1024 ? ex.Message[..1024] : ex.Message);
            }
        }

        Console.WriteLine($"\n  Done. Processed {processed} games, imported {totalReviews} reviews total.");
    }

    async Task<int> ImportReviewsAsync(ulong softwareId, List<Models.ParsedCriticReview> reviews)
    {
        await using var ctx = await _contextFactory.CreateDbContextAsync();

        // Load existing reviews for deduplication
        var existing = await ctx.SoftwareCriticReviews
                                .Where(r => r.SoftwareId == softwareId)
                                .Select(r => new { r.MagazineId, r.PlatformId })
                                .ToListAsync();

        var existingKeys = new HashSet<(long, ulong?)>(existing.Select(e => (e.MagazineId, e.PlatformId)));

        int imported = 0;

        foreach(var review in reviews)
        {
            // Match magazine
            var (magazineId, matchType) = await _magazineMatcher.MatchOrCreateAsync(review.PublicationName);

            if(magazineId == 0)
            {
                Console.WriteLine($"    \e[33mSkipping review — could not match magazine \"{review.PublicationName}\"\e[0m");

                continue;
            }

            // Match platform
            SoftwarePlatform platform = null;

            if(!string.IsNullOrWhiteSpace(review.PlatformName))
                platform = await _platformMatcher.MatchOrCreateAsync(review.PlatformName);

            ulong? platformId = platform?.Id;

            // Deduplicate
            if(existingKeys.Contains((magazineId, platformId)))
                continue;

            // Parse date
            DateTime?     reviewDate      = null;
            DatePrecision datePrecision   = DatePrecision.Full;

            if(!string.IsNullOrWhiteSpace(review.ReviewDate))
                (reviewDate, datePrecision) = ParseReviewDate(review.ReviewDate);

            var dbReview = new SoftwareCriticReview
            {
                SoftwareId          = softwareId,
                MagazineId          = magazineId,
                PlatformId          = platformId,
                NormalizedScore     = review.NormalizedScore,
                OriginalScore       = review.OriginalScore,
                OriginalScoreMaximum = review.OriginalScoreMaximum,
                ReviewText          = review.ReviewText,
                ReviewDate          = reviewDate,
                ReviewDatePrecision = datePrecision,
                ReviewUrl           = review.ReviewUrl
            };

            ctx.SoftwareCriticReviews.Add(dbReview);
            existingKeys.Add((magazineId, platformId));
            imported++;
        }

        if(imported > 0)
            await ctx.SaveChangesAsync();

        return imported;
    }

    static (DateTime? date, DatePrecision precision) ParseReviewDate(string dateStr)
    {
        if(string.IsNullOrWhiteSpace(dateStr))
            return (null, DatePrecision.Full);

        dateStr = dateStr.Trim();

        // Try YYYY-MM-DD
        if(DateTime.TryParseExact(dateStr, "yyyy-MM-dd",
                                  CultureInfo.InvariantCulture,
                                  DateTimeStyles.None, out var fullDate))
            return (fullDate, DatePrecision.Full);

        // Try YYYY-MM
        if(DateTime.TryParseExact(dateStr, "yyyy-MM",
                                  CultureInfo.InvariantCulture,
                                  DateTimeStyles.None, out var monthDate))
            return (monthDate, DatePrecision.MonthYear);

        // Try "Mon, YYYY" (old MobyGames format, e.g. "Jan, 1990")
        if(DateTime.TryParseExact(dateStr, "MMM, yyyy",
                                  CultureInfo.InvariantCulture,
                                  DateTimeStyles.None, out var monYearDate))
            return (monYearDate, DatePrecision.MonthYear);

        // Try "Mon YYYY" without comma (e.g. "Jan 1990")
        if(DateTime.TryParseExact(dateStr, "MMM yyyy",
                                  CultureInfo.InvariantCulture,
                                  DateTimeStyles.None, out var monYearDate2))
            return (monYearDate2, DatePrecision.MonthYear);

        // Try YYYY only
        if(int.TryParse(dateStr, out int year) && year is > 1900 and < 2100)
            return (new DateTime(year, 1, 1), DatePrecision.YearOnly);

        return (null, DatePrecision.Full);
    }
}
