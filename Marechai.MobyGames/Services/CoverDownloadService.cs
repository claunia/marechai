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
    readonly CoverStateService                 _coverStateService;
    readonly MobyGamesHttpClient               _httpClient;
    readonly string                            _assetRootPath;

    public CoverDownloadService(
        IDbContextFactory<MarechaiContext> contextFactory,
        SourceDatabaseService             sourceDb,
        CoverStateService                 coverStateService,
        MobyGamesHttpClient               httpClient,
        string                            assetRootPath)
    {
        _contextFactory    = contextFactory;
        _sourceDb          = sourceDb;
        _coverStateService = coverStateService;
        _httpClient        = httpClient;
        _assetRootPath     = assetRootPath;
    }

    public async Task RunAsync(int batchSize, bool dryRun, bool downloadOnly = false)
    {
        Console.WriteLine(dryRun
                              ? "\n  \e[33;1m[DRY RUN]\e[0m Parsing covers without downloading...\n"
                              : downloadOnly
                                  ? "\n  Starting cover download \e[33;1m(--download-only: conversion skipped, writing to photos-new/)\e[0m...\n"
                                  : "\n  Starting cover download...\n");

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

        var c              = new MediaCounters();
        int gamesProcessed = 0;
        int total          = Math.Min(batchSize, importedGames.Count);

        foreach(var game in importedGames.Take(batchSize))
        {
            gamesProcessed++;

            // Fetch all chunks for this game from source DB
            var rows = await _sourceDb.GetRowsForGameAsync(game.MobyGameId);

            bool ok = await ProcessGameAsync(game, rows, processedUrls, dryRun, downloadOnly, c,
                                             $"[{gamesProcessed}/{total}]");

            if(!ok) break;
        }

        // Print summary
        Console.WriteLine("\n  ────────────────────────────────────");

        if(dryRun)
        {
            Console.WriteLine("  \e[33;1m[DRY RUN]\e[0m No changes made");
            Console.WriteLine($"    Games scanned:       {gamesProcessed}");
            Console.WriteLine($"    Total covers found:  {c.Total}");
            Console.WriteLine($"    Would download:      {c.WouldAdd}");
        }
        else
        {
            Console.WriteLine($"    Games processed:     {gamesProcessed}");
            Console.WriteLine($"    Total covers found:  {c.Total}");
            Console.WriteLine($"    Downloaded:          {c.Added}");
            Console.WriteLine($"    Skipped (existing):  {c.Skipped}");
            Console.WriteLine($"    Failed:              {c.Failed}");
        }

        Console.WriteLine("  ────────────────────────────────────\n");
    }

    /// <summary>
    ///     Downloads every cover of ONE imported game that is not yet in
    ///     <see cref="MobyGamesCoverDownloadState" /> as <c>Downloaded</c>. <paramref name="rows" />
    ///     are the game's raw chunks (the CoverArt tab and, for the display name, the Main tab);
    ///     they may come from the source DB (batch mode) or from a live re-download held in memory
    ///     (<c>update-year</c>). Dedupes by MobyGames cover detail-page URL. Returns <c>false</c>
    ///     when the caller must abort the whole run (disk space exhausted).
    /// </summary>
    public async Task<bool> ProcessGameAsync(MobyGamesImportState game, List<MobyGamesRawRow> rows, HashSet<string> processedUrls,
        bool dryRun, bool downloadOnly, MediaCounters c, string progress = "")
    {
        // When --download-only is set, originals are written to `photos-new/` instead of
        // `photos/` so the operator can rsync them to a separate conversion host without
        // colliding with the existing converted-asset tree.
        string photosRoot = downloadOnly ? "photos-new" : "photos";


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
                Console.WriteLine($"  {progress} {game.MobyGameId}: No cover art tab");

            return true;
        }

        // Parse cover groups from HTML — new layout uses the post-2023 redesign.
        var coverGroups = isNewLayout
                              ? NewSite.CoverArtTabParser.Parse(coverHtml)
                              : CoverArtTabParser.Parse(coverHtml);

        if(coverGroups.Count == 0)
        {
            if(dryRun)
                Console.WriteLine($"  {progress} {game.MobyGameId}: No cover groups found in HTML");

            return true;
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

        Console.WriteLine($"\n  {progress} \e[36;1m{gameName}\e[0m ({game.MobyGameId})");
        Console.WriteLine($"    {coverGroups.Count} cover group(s), {coverGroups.Sum(g => g.Covers.Count)} total cover(s)");

        foreach(var group in coverGroups)
        {

            c.Total += group.Covers.Count;

            Console.WriteLine($"\n    Platform: {group.Platform}");
            Console.WriteLine($"    Countries: {string.Join(", ", group.Countries)}");

            if(!string.IsNullOrWhiteSpace(group.Packaging))
                Console.WriteLine($"    Packaging: {group.Packaging}");

            // Covers are no longer attached to a specific SoftwareRelease at import time —
            // a region-specific cover is evidence a matching release MAY exist, not proof of
            // which one. Every cover is downloaded and clustered by Software + GroupId
            // instead; correct release attribution (if any) is left to manual curation via
            // the admin cover UI's "assign group to release" action.
            foreach(var cover in group.Covers)
            {
                string coverTypeStr = cover.Type;
                var    coverType     = MapCoverType(coverTypeStr);

                if(dryRun)
                {
                    // With a populated processedUrls set (update-year passes the real one) only
                    // genuinely new covers are reported as "would download".
                    if(processedUrls.Contains(cover.DetailPageUrl))
                    {
                        c.Skipped++;

                        continue;
                    }

                    Console.WriteLine($"      {coverTypeStr,-25} cover-{cover.CoverId} group {group.GroupId}");
                    c.WouldAdd++;

                    continue;
                }

                // Check if already downloaded
                if(processedUrls.Contains(cover.DetailPageUrl))
                {
                    c.Skipped++;

                    continue;
                }

                // Check state table
                var existingState = await _coverStateService.GetStateByCoverUrlAsync(cover.DetailPageUrl);

                if(existingState is not null && existingState.Status == MobyGamesCoverDownloadStatus.Downloaded)
                {
                    c.Skipped++;

                    continue;
                }

                // Create or get state record
                if(existingState is null)
                {
                    existingState = new MobyGamesCoverDownloadState
                    {
                        MobyGameId   = game.MobyGameId,
                        SoftwareId   = game.SoftwareId!.Value,
                        CoverPageUrl = cover.DetailPageUrl,
                        CoverType    = coverTypeStr,
                        Platform     = Truncate(group.Platform, 256),
                        Countries    = string.Join(", ", group.Countries),
                        GroupId      = group.GroupId,
                        Status       = MobyGamesCoverDownloadStatus.Pending
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
                    c.Failed++;

                    continue;
                }

                Console.Write(isHighRes ? " downloading (high res)..." : " downloading...");

                // Download the original image
                // Check free disk space before downloading
                if(!HasSufficientDiskSpace(_assetRootPath))
                {
                    Console.WriteLine("\n\n  \e[31;1mABORTING: Less than 100 MB free disk space on target volume.\e[0m\n");
                    return false;
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
                    c.Failed++;

                    continue;
                }

                string originalFilePath = $"{destBasePath}.{extension}";

                // Create SoftwareCover record
                await using var coverContext = await _contextFactory.CreateDbContextAsync();

                if(!string.IsNullOrWhiteSpace(group.GroupId))
                {
                    bool groupExists =
                        await coverContext.SoftwareCoverGroups.AnyAsync(g => g.Id == group.GroupId);

                    if(!groupExists)
                        coverContext.SoftwareCoverGroups.Add(new SoftwareCoverGroup
                        {
                            Id           = group.GroupId,
                            PlatformName = Truncate(group.Platform, 256)
                        });
                }

                var softwareCover = new SoftwareCover
                {
                    Id                = coverId,
                    SoftwareId        = game.SoftwareId!.Value,
                    GroupId           = group.GroupId,
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

                c.Added++;
                c.WouldAdd++;
            }
        }

        return true;
    }


    /// <summary>
    ///     One-time/idempotent remediation for covers downloaded before the importer stopped
    ///     forcing every cover onto a (possibly wrong) <see cref="SoftwareRelease" />. Every
    ///     row in <see cref="MobyGamesCoverDownloadState" /> that reached
    ///     <see cref="MobyGamesCoverDownloadStatus.Downloaded" /> and has a linked
    ///     <see cref="SoftwareCover" /> already carries everything needed to fix it — no
    ///     network access required. The cover's <c>SoftwareReleaseId</c> is cleared and its
    ///     <c>SoftwareId</c>/<c>GroupId</c> are (re)populated from the state row, so it stops
    ///     sitting on a release it was never actually evidence for and becomes correctly
    ///     clustered with the rest of its original MobyGames cover group instead. The
    ///     <see cref="SoftwareCoverGroup" /> lookup (GroupId -> platform name) is also
    ///     backfilled from the state row's <c>Platform</c>, so the UI can build a
    ///     "Platform (#GroupId)" label instead of falling back to "Unknown".
    ///     Re-running this after a real pass is a no-op for already-fixed rows.
    /// </summary>
    public async Task RepairMisassignedCoversAsync(bool dryRun, int batchSize = 1000)
    {
        Console.WriteLine(dryRun
                              ? "\n  \e[33;1m[DRY RUN]\e[0m Scanning for misassigned covers...\n"
                              : "\n  Repairing misassigned covers...\n");

        await using var context = await _contextFactory.CreateDbContextAsync();

        int scanned  = 0;
        int detached = 0;
        int skipped  = 0;
        int missing  = 0;
        long lastId  = 0;

        HashSet<string> knownGroupIds =
            (await context.SoftwareCoverGroups.Select(g => g.Id).ToListAsync()).ToHashSet();

        while(true)
        {
            List<MobyGamesCoverDownloadState> batch = await context.MobyGamesCoverDownloadStates
                                                                    .Where(s => s.Id > lastId &&
                                                                                s.Status ==
                                                                                MobyGamesCoverDownloadStatus
                                                                                   .Downloaded &&
                                                                                s.SoftwareCoverId != null)
                                                                    .OrderBy(s => s.Id)
                                                                    .Take(batchSize)
                                                                    .ToListAsync();

            if(batch.Count == 0) break;

            lastId = batch[^1].Id;

            HashSet<ulong> existingSoftwareIds =
                (await context.Softwares.Where(sw => batch.Select(s => s.SoftwareId).Contains(sw.Id))
                              .Select(sw => sw.Id)
                              .ToListAsync()).ToHashSet();

            foreach(MobyGamesCoverDownloadState state in batch)
            {
                scanned++;

                SoftwareCover cover = await context.SoftwareCovers.FirstOrDefaultAsync(c => c.Id == state.SoftwareCoverId);

                if(cover is null)
                {
                    skipped++;

                    continue;
                }

                if(!existingSoftwareIds.Contains(state.SoftwareId))
                {
                    Console.WriteLine($"    [SKIP] cover {cover.Id}: state references missing Software " +
                                       $"{state.SoftwareId} (likely merged/deleted) - leaving untouched");

                    missing++;

                    continue;
                }

                if(!string.IsNullOrWhiteSpace(state.GroupId) && !knownGroupIds.Contains(state.GroupId))
                {
                    knownGroupIds.Add(state.GroupId);

                    if(!dryRun)
                        context.SoftwareCoverGroups.Add(new SoftwareCoverGroup
                        {
                            Id           = state.GroupId,
                            PlatformName = state.Platform
                        });
                }

                bool alreadyCorrect = cover.SoftwareReleaseId is null &&
                                      cover.SoftwareId == state.SoftwareId &&
                                      cover.GroupId    == state.GroupId;

                if(alreadyCorrect)
                {
                    skipped++;

                    continue;
                }

                Console.WriteLine($"    [DETACH] cover {cover.Id}: release {cover.SoftwareReleaseId} -> " +
                                   $"software {state.SoftwareId}, group {state.GroupId ?? "(none)"}, " +
                                   $"platform {state.Platform ?? "(none)"}");

                if(!dryRun)
                {
                    cover.SoftwareReleaseId = null;
                    cover.SoftwareId        = state.SoftwareId;
                    cover.GroupId           = state.GroupId;
                }

                detached++;
            }

            if(!dryRun) await context.SaveChangesAsync();
        }

        Console.WriteLine("\n  ────────────────────────────────────");
        Console.WriteLine(dryRun ? "  \e[33;1m[DRY RUN]\e[0m No changes made" : "  Repair complete");
        Console.WriteLine($"    Scanned:        {scanned}");
        Console.WriteLine($"    Detached/fixed: {detached}");
        Console.WriteLine($"    Already correct/skipped: {skipped}");
        Console.WriteLine($"    Skipped (missing Software): {missing}");
        Console.WriteLine("  ────────────────────────────────────\n");
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

    static string Truncate(string value, int maxLength) =>
        string.IsNullOrEmpty(value) ? value : value.Length <= maxLength ? value : value[..maxLength];
}
