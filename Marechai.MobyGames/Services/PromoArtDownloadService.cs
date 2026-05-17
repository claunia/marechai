using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Database.Models;
using Marechai.MobyGames.Models;
using Marechai.MobyGames.Parsers;
using Microsoft.EntityFrameworkCore;

namespace Marechai.MobyGames.Services;

public class PromoArtDownloadService
{
    const string PromoArtItemName = "software-promo-art";

    readonly IDbContextFactory<MarechaiContext> _contextFactory;
    readonly SourceDatabaseService             _sourceDb;
    readonly PromoArtStateService              _stateService;
    readonly MobyGamesHttpClient               _httpClient;
    readonly string                            _assetRootPath;

    public PromoArtDownloadService(
        IDbContextFactory<MarechaiContext> contextFactory,
        SourceDatabaseService             sourceDb,
        PromoArtStateService              stateService,
        MobyGamesHttpClient               httpClient,
        string                            assetRootPath)
    {
        _contextFactory = contextFactory;
        _sourceDb       = sourceDb;
        _stateService   = stateService;
        _httpClient     = httpClient;
        _assetRootPath  = assetRootPath;
    }

    public async Task RunAsync(int batchSize, bool dryRun)
    {
        Console.WriteLine(dryRun
                              ? "\n  \e[33;1m[DRY RUN]\e[0m Parsing promo art without downloading...\n"
                              : "\n  Starting promo art download...\n");

        await using var context = await _contextFactory.CreateDbContextAsync();

        // Only consider import states whose SoftwareId still exists in Softwares — stale states
        // (Software deleted after import) would otherwise cause an FK violation on insert.
        var importedGames = await context.MobyGamesImportStates
                                         .Where(s => s.Status     == MobyGamesImportStatus.Imported &&
                                                     s.SoftwareId != null &&
                                                     context.Softwares.Any(sw => sw.Id == s.SoftwareId.Value))
                                         .OrderBy(s => s.MobyGameId)
                                         .ToListAsync();

        Console.WriteLine($"  Found {importedGames.Count} imported games");

        var processedUrls = dryRun ? new HashSet<string>() : await _stateService.GetProcessedPromoUrlsAsync();

        Console.WriteLine($"  Already downloaded: {processedUrls.Count} promo images\n");

        int totalImages     = 0;
        int downloadedCount = 0;
        int skippedCount    = 0;
        int failedCount     = 0;
        int noPromoPage     = 0;
        int gamesProcessed  = 0;
        bool aborted        = false;

        foreach(var game in importedGames.Take(batchSize))
        {
            if(aborted) break;
            gamesProcessed++;

            var rows = await _sourceDb.GetRowsForGameAsync(game.MobyGameId);

            // Find the promo art page chunk (new MobyGames HTML stored by scraper)
            string promoHtml = null;

            foreach(var row in rows)
            {
                // New site promo pages contain /promo/group- pattern
                if(row.Body.Contains("/promo/group-"))
                {
                    promoHtml = row.Body;
                    break;
                }
            }

            if(promoHtml is null)
            {
                noPromoPage++;

                if(dryRun || noPromoPage <= 10)
                    Console.WriteLine($"  [{gamesProcessed}/{Math.Min(batchSize, importedGames.Count)}] {game.MobyGameId}: No scraped promo page (run scrape-promo-pages first)");

                if(noPromoPage == 10 && !dryRun)
                    Console.WriteLine("  ... suppressing further 'no promo page' messages ...");

                continue;
            }

            var promoGroups = PromoArtPageParser.Parse(promoHtml);

            if(promoGroups.Count == 0)
            {
                Console.WriteLine($"  [{gamesProcessed}/{Math.Min(batchSize, importedGames.Count)}] {game.MobyGameId}: Promo page found but no groups parsed");
                continue;
            }

            int imageCount = promoGroups.Sum(g => g.Images.Count);

            Console.WriteLine($"\n  [{gamesProcessed}] \e[36;1m{game.MobyGameId}\e[0m — {promoGroups.Count} group(s), {imageCount} image(s)");

            foreach(var group in promoGroups)
            {
                if(aborted) break;

                totalImages += group.Images.Count;

                Console.WriteLine($"    Group: {group.GroupName} ({group.Images.Count} images)");

                // Get or create group in DB
                int groupId = 0;

                if(!dryRun)
                    groupId = await GetOrCreateGroupAsync(group.GroupName);

                foreach(var image in group.Images)
                {
                    if(dryRun)
                    {
                        Console.WriteLine($"      image-{image.ImageId}: {image.Caption ?? "(no caption)"}");
                        continue;
                    }

                    string dedupeKey = image.DetailPageUrl ?? $"image-{image.ImageId}";

                    if(processedUrls.Contains(dedupeKey))
                    {
                        skippedCount++;
                        continue;
                    }

                    var existingState = await _stateService.GetStateByPromoUrlAsync(dedupeKey);

                    if(existingState is not null && existingState.Status == MobyGamesCoverDownloadStatus.Downloaded)
                    {
                        skippedCount++;
                        continue;
                    }

                    if(existingState is null)
                    {
                        existingState = new MobyGamesPromoArtDownloadState
                        {
                            MobyGameId   = game.MobyGameId,
                            SoftwareId   = game.SoftwareId!.Value,
                            PromoPageUrl = dedupeKey,
                            Caption      = image.Caption,
                            GroupName    = group.GroupName,
                            Status       = MobyGamesCoverDownloadStatus.Pending
                        };

                        await _stateService.CreateStateAsync(existingState);
                    }

                    // Visit the promo-art detail page so we can extract the MobyPlus
                    // <a download> link (numeric IDs in the original-resolution URL DIFFER from
                    // the thumbnail URL, so we MUST parse the detail page).
                    Console.Write($"      Fetching detail for image-{image.ImageId}...");

                    string detailHtml = image.DetailPageUrl is null
                                            ? null
                                            : await _httpClient.FetchPageAsync(image.DetailPageUrl);

                    if(detailHtml is null)
                    {
                        Console.WriteLine(" \e[31mFAILED\e[0m (detail page fetch failed)");

                        existingState.Status       = MobyGamesCoverDownloadStatus.Failed;
                        existingState.ErrorMessage = "Detail page fetch failed";
                        existingState.ProcessedOn  = DateTime.UtcNow;
                        await _stateService.UpdateStateAsync(existingState);
                        failedCount++;

                        continue;
                    }

                    string originalUrl;
                    bool   isHighRes;
                    (originalUrl, isHighRes) = MobyGamesHttpClient.ExtractFullSizeImageUrl(detailHtml);

                    if(originalUrl is null)
                    {
                        Console.WriteLine(" \e[31mFAILED\e[0m (no image URL found in detail page)");

                        existingState.Status       = MobyGamesCoverDownloadStatus.Failed;
                        existingState.ErrorMessage = "No full-size image URL found in detail page";
                        existingState.ProcessedOn  = DateTime.UtcNow;
                        await _stateService.UpdateStateAsync(existingState);
                        failedCount++;

                        continue;
                    }

                    if(!HasSufficientDiskSpace(_assetRootPath))
                    {
                        Console.WriteLine("\n\n  \e[31;1mABORTING: Less than 100 MB free disk space.\e[0m\n");
                        aborted = true;
                        break;
                    }

                    Console.Write(isHighRes ? " downloading (high res)..." : " downloading...");

                    var    promoArtId  = Guid.NewGuid();
                    string originalsDir = Path.Combine(_assetRootPath, "photos", PromoArtItemName, "originals");
                    string destBasePath = Path.Combine(originalsDir, promoArtId.ToString());

                    string extension = await _httpClient.DownloadImageAsync(originalUrl, destBasePath);

                    if(extension is null)
                    {
                        Console.WriteLine(" \e[31mFAILED\e[0m (download failed)");

                        existingState.Status       = MobyGamesCoverDownloadStatus.Failed;
                        existingState.ErrorMessage = "Image download failed";
                        existingState.ProcessedOn  = DateTime.UtcNow;
                        await _stateService.UpdateStateAsync(existingState);
                        failedCount++;

                        continue;
                    }

                    string originalFilePath = $"{destBasePath}.{extension}";

                    // Create SoftwarePromoArt record
                    await using var dbContext = await _contextFactory.CreateDbContextAsync();

                    // Defensive: re-check the FK target exists (Software may have been deleted
                    // between the initial filter and now). Avoids crashing the whole batch on FK
                    // violation when the import state is stale.
                    bool softwareExists = await dbContext.Softwares
                                                         .AnyAsync(sw => sw.Id == game.SoftwareId!.Value);

                    if(!softwareExists)
                    {
                        Console.WriteLine($" \e[33mSKIPPED\e[0m (Software {game.SoftwareId} no longer exists)");

                        // Best-effort: clean up the downloaded original since we won't link it.
                        try { File.Delete(originalFilePath); } catch { /* ignore */ }

                        existingState.Status       = MobyGamesCoverDownloadStatus.Failed;
                        existingState.ErrorMessage = $"Software {game.SoftwareId} no longer exists";
                        existingState.ProcessedOn  = DateTime.UtcNow;
                        await _stateService.UpdateStateAsync(existingState);
                        failedCount++;

                        continue;
                    }

                    var promoArt = new SoftwarePromoArt
                    {
                        Id                = promoArtId,
                        SoftwareId        = game.SoftwareId!.Value,
                        GroupId           = groupId,
                        Caption           = image.Caption,
                        OriginalExtension = extension
                    };

                    dbContext.SoftwarePromoArt.Add(promoArt);
                    await dbContext.SaveChangesAsync();

                    Console.Write(" converting...");

                    try
                    {
                        ImageConverter.ConvertAll(_assetRootPath, promoArtId, originalFilePath, extension,
                                                 PromoArtItemName);

                        Console.WriteLine(" \e[32mOK\e[0m");
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine($" \e[33mconversion warning: {ex}\e[0m");
                    }

                    existingState.Status             = MobyGamesCoverDownloadStatus.Downloaded;
                    existingState.SoftwarePromoArtId  = promoArtId;
                    existingState.OriginalUrl         = originalUrl;
                    existingState.ProcessedOn         = DateTime.UtcNow;
                    await _stateService.UpdateStateAsync(existingState);

                    downloadedCount++;
                }
            }
        }

        Console.WriteLine("\n  ────────────────────────────────────");

        if(dryRun)
        {
            Console.WriteLine("  \e[33;1m[DRY RUN]\e[0m No changes made");
            Console.WriteLine($"    Games scanned:       {gamesProcessed}");
            Console.WriteLine($"    No promo page:       {noPromoPage}");
            Console.WriteLine($"    Total images found:  {totalImages}");
        }
        else
        {
            Console.WriteLine($"    Games processed:     {gamesProcessed}");
            Console.WriteLine($"    No promo page:       {noPromoPage}");
            Console.WriteLine($"    Total images found:  {totalImages}");
            Console.WriteLine($"    Downloaded:          {downloadedCount}");
            Console.WriteLine($"    Skipped (existing):  {skippedCount}");
            Console.WriteLine($"    Failed:              {failedCount}");
        }

        Console.WriteLine("  ────────────────────────────────────\n");
    }

    async Task<int> GetOrCreateGroupAsync(string groupName)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var existing = await context.SoftwarePromoArtGroups
                                    .FirstOrDefaultAsync(g => g.Name == groupName);

        if(existing is not null) return existing.Id;

        var newGroup = new SoftwarePromoArtGroup { Name = groupName };

        context.SoftwarePromoArtGroups.Add(newGroup);
        await context.SaveChangesAsync();

        return newGroup.Id;
    }

    const long MinFreeSpaceBytes = 100 * 1024 * 1024;

    static bool HasSufficientDiskSpace(string path)
    {
        try
        {
            var driveInfo = new DriveInfo(Path.GetPathRoot(Path.GetFullPath(path))!);

            return driveInfo.AvailableFreeSpace > MinFreeSpaceBytes;
        }
        catch
        {
            return true;
        }
    }
}
