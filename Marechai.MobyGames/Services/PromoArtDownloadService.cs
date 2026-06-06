using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Database.Models;
using Marechai.MobyGames.Models;
using Marechai.MobyGames.Parsers;
using NewSite = Marechai.MobyGames.Parsers.NewSite;
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

    public async Task RunAsync(int batchSize, bool dryRun, bool downloadOnly = false)
    {
        // When --download-only is set, originals are written to `photos-new/` instead of
        // `photos/` so the operator can rsync them to a separate conversion host without
        // colliding with the existing converted-asset tree.
        string photosRoot = downloadOnly ? "photos-new" : "photos";

        Console.WriteLine(dryRun
                              ? "\n  \e[33;1m[DRY RUN]\e[0m Parsing promo art without downloading...\n"
                              : downloadOnly
                                  ? $"\n  Starting promo art download \e[33;1m(--download-only: conversion skipped, writing to {photosRoot}/)\e[0m...\n"
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
        int parseFailed     = 0;
        int gamesProcessed  = 0;
        bool aborted        = false;

        foreach(var game in importedGames.Take(batchSize))
        {
            if(aborted) break;
            gamesProcessed++;

            // Fetch the promo art page directly from the fixed chunk slot
            string promoHtml = await _sourceDb.GetChunkBodyAsync(game.MobyGameId, NewGameRawFetcher.ChunkPromo);

            if(promoHtml is null)
            {
                noPromoPage++;

                if(dryRun || noPromoPage <= 10)
                    Console.WriteLine($"  [{gamesProcessed}/{Math.Min(batchSize, importedGames.Count)}] {game.MobyGameId}: No scraped promo page (run scrape-promo-pages first)");

                if(noPromoPage == 10 && !dryRun)
                    Console.WriteLine("  ... suppressing further 'no promo page' messages ...");

                continue;
            }

            var promoGroups = NewSite.PromoArtTabParser.Parse(promoHtml);

            if(promoGroups.Count == 0)
            {
                parseFailed++;

                if(dryRun || parseFailed <= 20)
                    Console.WriteLine($"  [{gamesProcessed}/{Math.Min(batchSize, importedGames.Count)}] {game.MobyGameId}: Promo page found but no groups parsed");

                if(parseFailed == 20 && !dryRun)
                    Console.WriteLine("  ... suppressing further 'no groups parsed' messages ...");

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
                    groupId = await GetOrCreateGroupAsync(group.GroupName, group.GroupId);

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
                    string originalsDir = Path.Combine(_assetRootPath, photosRoot, PromoArtItemName, "originals");
                    Directory.CreateDirectory(originalsDir);
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

                    if(downloadOnly)
                    {
                        Console.WriteLine(" \e[33mskipped conversion\e[0m");
                    }
                    else
                    {
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
            Console.WriteLine($"    Parse failed:        {parseFailed}");
            Console.WriteLine($"    Total images found:  {totalImages}");
        }
        else
        {
            Console.WriteLine($"    Games processed:     {gamesProcessed}");
            Console.WriteLine($"    No promo page:       {noPromoPage}");
            Console.WriteLine($"    Parse failed:        {parseFailed}");
            Console.WriteLine($"    Total images found:  {totalImages}");
            Console.WriteLine($"    Downloaded:          {downloadedCount}");
            Console.WriteLine($"    Skipped (existing):  {skippedCount}");
            Console.WriteLine($"    Failed:              {failedCount}");
        }

        Console.WriteLine("  ────────────────────────────────────\n");
    }

    async Task<int> GetOrCreateGroupAsync(string groupName, string groupId = null)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        // If we have a groupId, check if a previous run created a bad "Group {id}" entry
        // and rename it to the real name. Skip if the current name IS the bad pattern
        // (regex fallback couldn't extract the real name — nothing to repair).
        if(groupId is not null)
        {
            string badName = $"Group {groupId}";

            if(groupName != badName)
            {
                var badEntry = await context.SoftwarePromoArtGroups
                                            .FirstOrDefaultAsync(g => g.Name == badName);

                if(badEntry is not null)
                {
                    // Check if the real name already exists too
                    var realEntry = await context.SoftwarePromoArtGroups
                                                 .FirstOrDefaultAsync(g => g.Name == groupName);

                    if(realEntry is null)
                    {
                        // Just rename the bad entry
                        badEntry.Name = groupName;
                        await context.SaveChangesAsync();
                        Console.WriteLine($"      \e[33mRepaired\e[0m group \"{badName}\" → \"{groupName}\"");

                        return badEntry.Id;
                    }

                    // Both exist — migrate promo art from bad group to real group, then delete bad
                    var orphaned = await context.SoftwarePromoArt
                                                .Where(p => p.GroupId == badEntry.Id)
                                                .ToListAsync();

                    foreach(var art in orphaned)
                        art.GroupId = realEntry.Id;

                    context.SoftwarePromoArtGroups.Remove(badEntry);
                    await context.SaveChangesAsync();

                    if(orphaned.Count > 0)
                        Console.WriteLine($"      \e[33mRepaired\e[0m migrated {orphaned.Count} images from \"{badName}\" → \"{groupName}\"");

                    return realEntry.Id;
                }
            }
        }

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
