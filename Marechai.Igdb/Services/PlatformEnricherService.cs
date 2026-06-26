using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Igdb.Services;

public class PlatformEnricherService
{
    readonly IDbContextFactory<MarechaiContext> _contextFactory;
    readonly string                              _assetRootPath;
    readonly HttpClient                          _httpClient;

    public PlatformEnricherService(IDbContextFactory<MarechaiContext> contextFactory, string assetRootPath)
    {
        _contextFactory = contextFactory;
        _assetRootPath  = assetRootPath;
        _httpClient     = new HttpClient();
    }

    public class Stats
    {
        public int PlatformsProcessed;
        public int PlatformsSkippedNoLocalMatch;
        public int LogosDownloaded;
        public int LogosSkippedAlreadyPresent;
    }

    public async Task<Stats> RunAsync(bool dryRun)
    {
        var stats = new Stats();

        await using var context = await _contextFactory.CreateDbContextAsync();

        var pending = await context.IgdbPlatforms
                                    .Where(p => p.MatchStatus == IgdbMatchStatus.Matched &&
                                                p.SoftwarePlatformId != null && !p.EnrichmentApplied)
                                    .ToListAsync();

        foreach(IgdbPlatform igdbPlatform in pending)
        {
            SoftwarePlatform platform =
                await context.SoftwarePlatforms.FirstOrDefaultAsync(p => p.Id == igdbPlatform.SoftwarePlatformId);

            if(platform == null)
            {
                stats.PlatformsSkippedNoLocalMatch++;

                Console.WriteLine(
                    $"  [{igdbPlatform.Id}] No local platform {igdbPlatform.SoftwarePlatformId} found, skipping.");

                continue;
            }

            stats.PlatformsProcessed++;

            if(platform.LogoId.HasValue)
                stats.LogosSkippedAlreadyPresent++;
            else if(!string.IsNullOrWhiteSpace(igdbPlatform.LogoImageId))
            {
                if(!dryRun)
                    await DownloadAndApplyLogoAsync(igdbPlatform.Id, platform, igdbPlatform.LogoImageId);

                stats.LogosDownloaded++;
            }

            if(!dryRun)
            {
                igdbPlatform.EnrichmentApplied = true;
                igdbPlatform.EnrichedOn        = DateTime.UtcNow;
            }
        }

        if(!dryRun)
            await context.SaveChangesAsync();

        return stats;
    }

    async Task DownloadAndApplyLogoAsync(int igdbId, SoftwarePlatform platform, string imageId)
    {
        byte[] bytes = await IgdbImageDownloader.DownloadBestAsync(_httpClient, imageId,
                                                                     IgdbImageDownloader.LogoSizeCandidates);

        if(bytes == null)
        {
            Console.WriteLine($"  [{igdbId}] Failed to download logo: no candidate size succeeded.");

            return;
        }

        var guid = Guid.NewGuid();

        string itemPhotosRoot         = Path.Combine(_assetRootPath, "photos", "platform-logos");
        string itemThumbsRoot         = Path.Combine(itemPhotosRoot, "thumbs");
        string itemOriginalPhotosRoot = Path.Combine(itemPhotosRoot, "originals");

        foreach(string dir in new[]
                {
                    itemPhotosRoot, itemThumbsRoot, itemOriginalPhotosRoot,
                    Path.Combine(itemThumbsRoot, "jpeg", "4k"), Path.Combine(itemPhotosRoot, "jpeg", "4k"),
                    Path.Combine(itemThumbsRoot, "webp", "4k"), Path.Combine(itemPhotosRoot, "webp", "4k"),
                    Path.Combine(itemThumbsRoot, "avif", "4k"), Path.Combine(itemPhotosRoot, "avif", "4k")
                })
            Directory.CreateDirectory(dir);

        string originalPath = Path.Combine(itemOriginalPhotosRoot, $"{guid}.jpg");
        await File.WriteAllBytesAsync(originalPath, bytes);

        foreach((string format, string outExt) in new[]
                {
                    ("jpeg", "jpg"), ("webp", "webp"), ("avif", "avif")
                })
        {
            string fullPath  = Path.Combine(itemPhotosRoot, format, "4k", $"{guid}.{outExt}");
            string thumbPath = Path.Combine(itemThumbsRoot, format, "4k", $"{guid}.{outExt}");

            ConvertUsingImageMagick(originalPath, fullPath,  256, 256);
            ConvertUsingImageMagick(originalPath, thumbPath, 64,  64);
        }

        platform.LogoId        = guid;
        platform.LogoExtension = "jpg";

        Console.WriteLine($"  [{igdbId}] Logo downloaded and converted for platform {platform.Id}.");
    }

    // Mirrors Photos.ConvertUsingImageMagick (Marechai.Server/Helpers/Photos.cs) without taking a
    // dependency on Marechai.Server, since Marechai.Igdb is a standalone console tool that doesn't
    // otherwise need ASP.NET Core.
    static void ConvertUsingImageMagick(string originalPath, string outputPath, int width, int height)
    {
        try
        {
            var p = new Process
            {
                StartInfo =
                {
                    FileName               = "magick",
                    CreateNoWindow         = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError  = true
                }
            };

            p.StartInfo.ArgumentList.Add("-limit");
            p.StartInfo.ArgumentList.Add("thread");
            p.StartInfo.ArgumentList.Add("1");
            p.StartInfo.ArgumentList.Add($"{originalPath}[0]");
            p.StartInfo.ArgumentList.Add("-resize");
            p.StartInfo.ArgumentList.Add($"{width}x{height}>");
            p.StartInfo.ArgumentList.Add("-strip");
            p.StartInfo.ArgumentList.Add("-quality");
            p.StartInfo.ArgumentList.Add("75");
            p.StartInfo.ArgumentList.Add(outputPath);
            p.StartInfo.Environment["MAGICK_THREAD_LIMIT"] = "1";

            p.Start();

            Task<string> stderrTask = p.StandardError.ReadToEndAsync();
            Task<string> stdoutTask = p.StandardOutput.ReadToEndAsync();
            p.WaitForExit();
            _ = stderrTask.GetAwaiter().GetResult();
            _ = stdoutTask.GetAwaiter().GetResult();

            if(p.ExitCode != 0)
                Console.Error.WriteLine($"convert failed for {outputPath}: exit {p.ExitCode}");
        }
        catch(Exception ex)
        {
            Console.Error.WriteLine($"convert spawn failed for {outputPath}: {ex.Message}");
        }
    }
}
