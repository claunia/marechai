using System;
using System.IO;
using System.Linq;

namespace Marechai.MobyGames.Services;

public enum ImageConversionItemType
{
    Covers,
    PromoArt,
    Screenshots
}

/// <summary>
/// Offline image conversion pass. Walks <c>photos/&lt;itemName&gt;/originals/</c> on disk
/// and, for each Guid-named original whose 8 converted outputs (JPEG/WEBP/AVIF/JXL ×
/// thumb/full at 4k) are not all present, runs <see cref="ImageConverter.ConvertAll"/>.
///
/// Designed to be runnable on a machine that has ONLY the <c>photos/</c> tree and
/// ImageMagick installed — no MariaDB, no MobyGames source DB, no HTTP client, no
/// headless browser. This lets the slow conversion step be offloaded to a more
/// powerful machine while downloads remain on the lightweight scraper host.
/// </summary>
public static class ImageConversionPassService
{
    public static string GetItemDirectoryName(ImageConversionItemType type) => type switch
    {
        ImageConversionItemType.Covers      => "software-covers",
        ImageConversionItemType.PromoArt    => "software-promo-art",
        ImageConversionItemType.Screenshots => "software-screenshots",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };

    /// <summary>
    /// Runs the conversion pass for a single item type. Returns the number of failed
    /// conversions (0 on success). Never throws for individual file failures — each
    /// ImageMagick error is counted and logged, the pass continues.
    /// </summary>
    public static int Run(string assetRootPath, ImageConversionItemType type, int batchSize, bool dryRun)
    {
        string itemName    = GetItemDirectoryName(type);
        string originalsDir = Path.Combine(assetRootPath, "photos", itemName, "originals");

        Console.WriteLine(dryRun
                              ? $"\n  \e[33;1m[DRY RUN]\e[0m Scanning {itemName} originals (no conversion will run)...\n"
                              : $"\n  Converting missing variants for {itemName}...\n");

        if(!Directory.Exists(originalsDir))
        {
            Console.WriteLine($"  \e[33mWarning: originals directory not found: {originalsDir}\e[0m");
            Console.WriteLine("  Skipping this item type.\n");

            return 0;
        }

        int scanned          = 0;
        int alreadyConverted = 0;
        int wouldConvert     = 0;
        int converted        = 0;
        int failed           = 0;
        int skippedFilename  = 0;

        // Enumerate originals in stable order so re-runs with `--batch-size` advance
        // predictably across a partially-converted tree.
        var files = Directory.EnumerateFiles(originalsDir, "*", SearchOption.TopDirectoryOnly)
                             .OrderBy(p => p, StringComparer.Ordinal);

        foreach(string filePath in files)
        {
            // Skip dotfiles and transient artefacts (e.g. partially-downloaded files).
            string fileName = Path.GetFileName(filePath);

            if(fileName.StartsWith(".", StringComparison.Ordinal) ||
               fileName.EndsWith(".partial", StringComparison.OrdinalIgnoreCase) ||
               fileName.EndsWith(".tmp", StringComparison.OrdinalIgnoreCase))
                continue;

            string stem      = Path.GetFileNameWithoutExtension(fileName);
            string extension = Path.GetExtension(fileName).TrimStart('.').ToLowerInvariant();

            if(string.IsNullOrEmpty(extension))
            {
                skippedFilename++;
                continue;
            }

            if(!Guid.TryParse(stem, out var id))
            {
                skippedFilename++;
                continue;
            }

            scanned++;

            if(ImageConverter.HasAllVariants(assetRootPath, id, itemName))
            {
                alreadyConverted++;
                continue;
            }

            if(dryRun)
            {
                wouldConvert++;

                if(wouldConvert <= 20)
                    Console.WriteLine($"    Would convert {id} ({extension})");
                else if(wouldConvert == 21)
                    Console.WriteLine("    ... (suppressing further dry-run entries) ...");

                continue;
            }

            // Per-type batch cap so long runs can be throttled. Matches the throttle
            // semantics of the download commands.
            if(batchSize > 0 && converted + failed >= batchSize)
            {
                Console.WriteLine($"    Reached --batch-size cap ({batchSize}); stopping this type.");
                break;
            }

            Console.Write($"    Converting {id} ({extension})...");

            try
            {
                ImageConverter.ConvertAll(assetRootPath, id, filePath, extension, itemName);
                converted++;
                Console.WriteLine(" \e[32mOK\e[0m");
            }
            catch(Exception ex)
            {
                failed++;
                Console.WriteLine($" \e[31mFAILED\e[0m ({ex.Message})");
            }
        }

        Console.WriteLine("\n  ────────────────────────────────────");
        Console.WriteLine($"  {itemName}");

        if(dryRun)
            Console.WriteLine("    \e[33;1m[DRY RUN]\e[0m No files were converted.");

        Console.WriteLine($"    Originals scanned:    {scanned}");

        if(skippedFilename > 0)
            Console.WriteLine($"    Non-Guid filenames:   {skippedFilename}");

        Console.WriteLine($"    Already converted:    {alreadyConverted}");

        if(dryRun)
            Console.WriteLine($"    Would convert:        {wouldConvert}");
        else
        {
            Console.WriteLine($"    Converted:            {converted}");
            Console.WriteLine($"    Failed:               {failed}");
        }

        Console.WriteLine("  ────────────────────────────────────\n");

        return failed;
    }
}
