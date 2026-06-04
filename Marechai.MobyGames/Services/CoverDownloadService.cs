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
using NewSite = Marechai.MobyGames.Parsers.NewSite;

namespace Marechai.MobyGames.Services;

public class CoverDownloadService
{
    readonly IDbContextFactory<MarechaiContext> _contextFactory;
    readonly SourceDatabaseService             _sourceDb;
    readonly PlatformMatcher                   _platformMatcher;
    readonly CountryMatcher                    _countryMatcher;
    readonly CoverStateService                 _coverStateService;
    readonly MobyGamesHttpClient               _httpClient;
    readonly string                            _assetRootPath;

    public CoverDownloadService(
        IDbContextFactory<MarechaiContext> contextFactory,
        SourceDatabaseService             sourceDb,
        PlatformMatcher                   platformMatcher,
        CountryMatcher                    countryMatcher,
        CoverStateService                 coverStateService,
        MobyGamesHttpClient               httpClient,
        string                            assetRootPath)
    {
        _contextFactory    = contextFactory;
        _sourceDb          = sourceDb;
        _platformMatcher   = platformMatcher;
        _countryMatcher    = countryMatcher;
        _coverStateService = coverStateService;
        _httpClient        = httpClient;
        _assetRootPath     = assetRootPath;
    }

    public async Task RunAsync(int batchSize, bool dryRun, bool downloadOnly = false)
    {
        // When --download-only is set, originals are written to `photos-new/` instead of
        // `photos/` so the operator can rsync them to a separate conversion host without
        // colliding with the existing converted-asset tree. The conversion stage on the
        // target host can then either merge `photos-new/` into `photos/` first or treat
        // it as its own asset root for `convert-images`.
        string photosRoot = downloadOnly ? "photos-new" : "photos";

        Console.WriteLine(dryRun
                              ? "\n  \e[33;1m[DRY RUN]\e[0m Parsing covers without downloading...\n"
                              : downloadOnly
                                  ? $"\n  Starting cover download \e[33;1m(--download-only: conversion skipped, writing to {photosRoot}/)\e[0m...\n"
                                  : "\n  Starting cover download...\n");

        // Load reference data
        Console.WriteLine("  Loading reference data...");
        await _platformMatcher.LoadAsync();
        await _countryMatcher.LoadAsync();

        // Get all imported games with SoftwareId
        await using var context = await _contextFactory.CreateDbContextAsync();

        var importedGames = await context.MobyGamesImportStates
                                         .Where(s => s.Status     == MobyGamesImportStatus.Imported &&
                                                     s.SoftwareId != null)
                                         .OrderBy(s => s.MobyGameId)
                                         .ToListAsync();

        Console.WriteLine($"  Found {importedGames.Count} imported games with SoftwareId");

        // Get already-processed cover URLs for quick skip
        var processedUrls = dryRun ? [] : await _coverStateService.GetProcessedCoverUrlsAsync();

        Console.WriteLine($"  Already downloaded: {processedUrls.Count} covers\n");

        int totalCovers     = 0;
        int matchedCovers   = 0;
        int unmatchedCovers = 0;
        int downloadedCount = 0;
        int skippedCount    = 0;
        int failedCount     = 0;
        int gamesProcessed  = 0;
        bool aborted        = false;

        foreach(var game in importedGames.Take(batchSize))
        {
            if(aborted) break;

            gamesProcessed++;

            // Fetch all chunks for this game from source DB
            var rows = await _sourceDb.GetRowsForGameAsync(game.MobyGameId);

            // Find the cover art tab chunk
            string  coverHtml   = null;
            bool    isNewLayout = false;

            foreach(var row in rows)
            {
                var (tab, layout) = TabDetector.DetectWithLayout(row.Body);

                if(tab == MobyTab.CoverArt)
                {
                    coverHtml   = row.Body;
                    isNewLayout = layout == MobyLayout.New;

                    break;
                }
            }

            if(coverHtml is null)
            {
                if(dryRun)
                    Console.WriteLine($"  [{gamesProcessed}/{Math.Min(batchSize, importedGames.Count)}] {game.MobyGameId}: No cover art tab");

                continue;
            }

            // Parse cover groups from HTML — new layout uses the post-2023 redesign.
            var coverGroups = isNewLayout
                                  ? NewSite.CoverArtTabParser.Parse(coverHtml)
                                  : CoverArtTabParser.Parse(coverHtml);

            if(coverGroups.Count == 0)
            {
                if(dryRun)
                    Console.WriteLine($"  [{gamesProcessed}/{Math.Min(batchSize, importedGames.Count)}] {game.MobyGameId}: No cover groups found in HTML");

                continue;
            }

            // Get the game name for display
            string gameName = null;

            foreach(var row in rows)
            {
                var (tab, layout) = TabDetector.DetectWithLayout(row.Body);

                if(tab != MobyTab.Main) continue;

                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(row.Body);

                // New layout uses <h1 class="mb-0"> with text content; old layout
                // uses <h1 class="niceHeaderTitle"><a>...</a>.
                if(layout == MobyLayout.New)
                {
                    var h1 = doc.DocumentNode.SelectSingleNode("//h1[contains(@class,'mb-0')]");

                    if(h1 is not null)
                        gameName = System.Net.WebUtility.HtmlDecode(h1.InnerText).Trim();
                }
                else
                {
                    var h1 = doc.DocumentNode.SelectSingleNode("//h1[contains(@class,'niceHeaderTitle')]//a");

                    if(h1 is not null)
                        gameName = System.Net.WebUtility.HtmlDecode(h1.InnerText).Trim();
                }

                break;
            }

            gameName ??= game.MobyGameId;

            Console.WriteLine($"\n  [{gamesProcessed}/{Math.Min(batchSize, importedGames.Count)}] \e[36;1m{gameName}\e[0m ({game.MobyGameId})");
            Console.WriteLine($"    {coverGroups.Count} cover group(s), {coverGroups.Sum(g => g.Covers.Count)} total cover(s)");

            foreach(var group in coverGroups)
            {
                if(aborted) break;

                totalCovers += group.Covers.Count;

                Console.WriteLine($"\n    Platform: {group.Platform}");
                Console.WriteLine($"    Countries: {string.Join(", ", group.Countries)}");

                if(!string.IsNullOrWhiteSpace(group.Packaging))
                    Console.WriteLine($"    Packaging: {group.Packaging}");

                // Match platform
                var platform = await _platformMatcher.MatchOrCreateAsync(group.Platform);

                // Match countries to UnM49
                var matchedCountries = new List<UnM49>();

                foreach(string country in group.Countries)
                {
                    var unm49 = _countryMatcher.Match(country);

                    if(unm49 is not null)
                        matchedCountries.Add(unm49);
                }

                // Find best matching SoftwareRelease
                var release = await FindBestReleaseAsync(
                    game.SoftwareId!.Value, platform?.Id, matchedCountries);

                if(release is not null)
                {
                    Console.WriteLine($"    \e[32mMatched release #{release.Id}\e[0m");
                }
                else
                {
                    Console.WriteLine($"    \e[33m[NO RELEASE]\e[0m");
                }

                foreach(var cover in group.Covers)
                {
                    string coverTypeStr = cover.Type;
                    var    coverType     = MapCoverType(coverTypeStr);

                    if(dryRun)
                    {
                        string releaseStr = release is not null
                                                ? $"→ Release #{release.Id}"
                                                : "\e[33m[NO RELEASE]\e[0m";

                        Console.WriteLine($"      {coverTypeStr,-25} cover-{cover.CoverId} {releaseStr}");

                        if(release is not null)
                            matchedCovers++;
                        else
                            unmatchedCovers++;

                        continue;
                    }

                    // Check if already downloaded
                    if(processedUrls.Contains(cover.DetailPageUrl))
                    {
                        skippedCount++;

                        continue;
                    }

                    // Check state table
                    var existingState = await _coverStateService.GetStateByCoverUrlAsync(cover.DetailPageUrl);

                    if(existingState is not null && existingState.Status == MobyGamesCoverDownloadStatus.Downloaded)
                    {
                        skippedCount++;

                        continue;
                    }

                    if(release is null)
                    {
                        // No matching release — track as NoRelease
                        if(existingState is null)
                        {
                            await _coverStateService.CreateStateAsync(new MobyGamesCoverDownloadState
                            {
                                MobyGameId   = game.MobyGameId,
                                SoftwareId   = game.SoftwareId!.Value,
                                CoverPageUrl = cover.DetailPageUrl,
                                CoverType    = coverTypeStr,
                                Platform     = group.Platform,
                                Countries    = string.Join(", ", group.Countries),
                                GroupId      = group.GroupId,
                                Status       = MobyGamesCoverDownloadStatus.NoRelease,
                                ProcessedOn  = DateTime.UtcNow
                            });
                        }

                        unmatchedCovers++;

                        continue;
                    }

                    // Create or get state record
                    if(existingState is null)
                    {
                        existingState = new MobyGamesCoverDownloadState
                        {
                            MobyGameId        = game.MobyGameId,
                            SoftwareId        = game.SoftwareId!.Value,
                            CoverPageUrl      = cover.DetailPageUrl,
                            CoverType         = coverTypeStr,
                            Platform          = group.Platform,
                            Countries         = string.Join(", ", group.Countries),
                            GroupId           = group.GroupId,
                            Status            = MobyGamesCoverDownloadStatus.Pending,
                            SoftwareReleaseId = release.Id
                        };

                        await _coverStateService.CreateStateAsync(existingState);
                    }

                    // Fetch cover detail page (authenticated)
                    Console.Write($"      Fetching detail for {coverTypeStr}...");

                    // Prefer the MobyPlus <a download> original on the detail page (logged-in users
                    // see a higher-resolution variant whose numeric IDs differ from the thumbnail).
                    // Fall back to /covers/s/→/covers/l/ rewriting when MobyPlus isn't available.
                    string originalUrl = null;
                    bool   isHighRes  = false;

                    if(cover.DetailPageUrl is not null)
                    {
                        string detailHtml = await _httpClient.FetchPageAsync(cover.DetailPageUrl);

                        if(detailHtml is not null)
                            (originalUrl, isHighRes) = MobyGamesHttpClient.ExtractFullSizeImageUrl(detailHtml);
                    }

                    originalUrl ??= MobyGamesHttpClient.GetLargeImageUrl(cover.ThumbnailUrl);

                    if(originalUrl is null)
                    {
                        Console.WriteLine(" \e[31mFAILED\e[0m (no thumbnail URL available)");

                        existingState.Status       = MobyGamesCoverDownloadStatus.Failed;
                        existingState.ErrorMessage = "No thumbnail URL to derive original from";
                        existingState.ProcessedOn  = DateTime.UtcNow;
                        await _coverStateService.UpdateStateAsync(existingState);
                        failedCount++;

                        continue;
                    }

                    Console.Write(isHighRes ? " downloading (high res)..." : " downloading...");

                    // Download the original image
                    // Check free disk space before downloading
                    if(!HasSufficientDiskSpace(_assetRootPath))
                    {
                        Console.WriteLine("\n\n  \e[31;1mABORTING: Less than 100 MB free disk space on target volume.\e[0m\n");
                        aborted = true;

                        break;
                    }

                    var    coverId       = Guid.NewGuid();
                    string originalsDir = Path.Combine(_assetRootPath, photosRoot, "software-covers", "originals");
                    Directory.CreateDirectory(originalsDir);
                    string destBasePath = Path.Combine(originalsDir, coverId.ToString());

                    string extension = await _httpClient.DownloadImageAsync(originalUrl, destBasePath);

                    if(extension is null)
                    {
                        Console.WriteLine(" \e[31mFAILED\e[0m (download failed)");

                        existingState.Status       = MobyGamesCoverDownloadStatus.Failed;
                        existingState.ErrorMessage = "Image download failed";
                        existingState.ProcessedOn  = DateTime.UtcNow;
                        await _coverStateService.UpdateStateAsync(existingState);
                        failedCount++;

                        continue;
                    }

                    string originalFilePath = $"{destBasePath}.{extension}";

                    // Create SoftwareCover record
                    await using var coverContext = await _contextFactory.CreateDbContextAsync();

                    var softwareCover = new SoftwareCover
                    {
                        Id                = coverId,
                        SoftwareReleaseId = release.Id,
                        Type              = coverType,
                        Caption           = coverTypeStr,
                        OriginalExtension = extension
                    };

                    coverContext.SoftwareCovers.Add(softwareCover);
                    await coverContext.SaveChangesAsync();

                    // Run image conversion (6 variants) unless the caller explicitly
                    // opted out via --download-only (the conversion pass can be offloaded
                    // to another machine via the `convert-images` command).
                    if(downloadOnly)
                    {
                        Console.WriteLine(" \e[33mskipped conversion\e[0m");
                    }
                    else
                    {
                        Console.Write(" converting...");

                        try
                        {
                            ImageConverter.ConvertAll(_assetRootPath, coverId, originalFilePath, extension);
                            Console.WriteLine(" \e[32mOK\e[0m");
                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine($" \e[33mconversion warning: {ex}\e[0m");
                        }
                    }

                    // Update state
                    existingState.Status          = MobyGamesCoverDownloadStatus.Downloaded;
                    existingState.SoftwareCoverId = coverId;
                    existingState.OriginalUrl     = originalUrl;
                    existingState.ProcessedOn     = DateTime.UtcNow;
                    await _coverStateService.UpdateStateAsync(existingState);

                    downloadedCount++;
                    matchedCovers++;
                }
            }
        }

        // Print summary
        Console.WriteLine("\n  ────────────────────────────────────");

        if(dryRun)
        {
            Console.WriteLine("  \e[33;1m[DRY RUN]\e[0m No changes made");
            Console.WriteLine($"    Games scanned:       {gamesProcessed}");
            Console.WriteLine($"    Total covers found:  {totalCovers}");
            Console.WriteLine($"    Matched to release:  {matchedCovers}");
            Console.WriteLine($"    No matching release: {unmatchedCovers}");
        }
        else
        {
            Console.WriteLine($"    Games processed:     {gamesProcessed}");
            Console.WriteLine($"    Total covers found:  {totalCovers}");
            Console.WriteLine($"    Downloaded:          {downloadedCount}");
            Console.WriteLine($"    Skipped (existing):  {skippedCount}");
            Console.WriteLine($"    Failed:              {failedCount}");
            Console.WriteLine($"    No matching release: {unmatchedCovers}");
        }

        Console.WriteLine("  ────────────────────────────────────\n");
    }

    async Task<SoftwareRelease> FindBestReleaseAsync(ulong softwareId, ulong? platformId,
                                                     List<UnM49> matchedCountries)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        // Get all releases for this software
        var releases = await context.SoftwareReleases
                                    .Where(r => r.SoftwareId == softwareId)
                                    .Include(r => r.Regions)
                                    .ToListAsync();

        if(releases.Count == 0) return null;

        // Filter by platform if we have one
        var candidates = platformId is not null
                             ? releases.Where(r => r.PlatformId == platformId).ToList()
                             : releases;

        // If platform filter emptied our candidates, fall back to all releases
        if(candidates.Count == 0)
            candidates = releases;

        if(candidates.Count == 1) return candidates[0];

        // Score by country overlap
        if(matchedCountries.Count > 0)
        {
            var countryIds = matchedCountries.Select(c => c.Id).ToHashSet();

            var scored = candidates
                        .Select(r => new
                         {
                             Release = r,
                             Score = r.Regions?.Count(reg => countryIds.Contains(reg.UnM49Id)) ?? 0
                         })
                        .OrderByDescending(x => x.Score)
                        .ToList();

            // Return the best match if it has any overlap
            if(scored[0].Score > 0) return scored[0].Release;
        }

        // Fallback: return the first candidate
        return candidates.FirstOrDefault();
    }

    static SoftwareCoverType MapCoverType(string mobyType)
    {
        if(string.IsNullOrWhiteSpace(mobyType)) return SoftwareCoverType.Other;

        string normalized = mobyType.Trim();

        if(normalized.Equals("Front Cover", StringComparison.OrdinalIgnoreCase))
            return SoftwareCoverType.Front;

        if(normalized.Equals("Back Cover", StringComparison.OrdinalIgnoreCase))
            return SoftwareCoverType.Back;

        if(normalized.StartsWith("Inside Cover", StringComparison.OrdinalIgnoreCase))
        {
            if(normalized.Contains("Right", StringComparison.OrdinalIgnoreCase) ||
               normalized.Contains("Back",  StringComparison.OrdinalIgnoreCase))
                return SoftwareCoverType.InsideBack;

            return SoftwareCoverType.InsideFront;
        }

        if(normalized.Equals("Media", StringComparison.OrdinalIgnoreCase) ||
           normalized.StartsWith("Media ", StringComparison.OrdinalIgnoreCase))
            return SoftwareCoverType.Media;

        if(normalized.StartsWith("Spine", StringComparison.OrdinalIgnoreCase))
            return SoftwareCoverType.Spine;

        if(normalized.Equals("Manual", StringComparison.OrdinalIgnoreCase))
            return SoftwareCoverType.Manual;

        return SoftwareCoverType.Other;
    }

    const long MinFreeSpaceBytes = 100 * 1024 * 1024; // 100 MB

    static bool HasSufficientDiskSpace(string path)
    {
        try
        {
            var driveInfo = new DriveInfo(Path.GetPathRoot(Path.GetFullPath(path))!);

            return driveInfo.AvailableFreeSpace > MinFreeSpaceBytes;
        }
        catch
        {
            // If we can't determine free space, allow continuing
            return true;
        }
    }
}
