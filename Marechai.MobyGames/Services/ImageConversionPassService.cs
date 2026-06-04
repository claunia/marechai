using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
    /// <param name="parallelism">
    /// Maximum number of concurrent <see cref="ImageConverter.ConvertAll" /> invocations.
    /// <c>0</c> means <see cref="Environment.ProcessorCount" /> (auto). <c>1</c> forces
    /// the legacy sequential behaviour. ImageMagick already uses multiple threads per file
    /// via OpenMP — the per-CPU cap here lets the operator dial that down on shared boxes.
    /// </param>
    public static int Run(string assetRootPath, ImageConversionItemType type, int batchSize, bool dryRun,
                          int parallelism)
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
        int skippedFilename  = 0;

        // Enumerate originals in stable order so re-runs with `--batch-size` advance
        // predictably across a partially-converted tree.
        var files = Directory.EnumerateFiles(originalsDir, "*", SearchOption.TopDirectoryOnly)
                             .OrderBy(p => p, StringComparer.Ordinal);

        // First pass: enumerate + filter into a list of work items. Doing the scan up front
        // (rather than streaming into the parallel loop) lets us honour --batch-size deterministically
        // and report `Originals scanned` accurately whether or not the cap is hit.
        var pending = new List<(Guid Id, string FilePath, string Extension)>();

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

            pending.Add((id, filePath, extension));

            // Per-type batch cap so long runs can be throttled. Matches the throttle
            // semantics of the download commands.
            if(batchSize > 0 && pending.Count >= batchSize)
            {
                Console.WriteLine($"    Reached --batch-size cap ({batchSize}); stopping enumeration for this type.");
                break;
            }
        }

        int convertedVariants = 0;
        int failedVariants    = 0;

        if(!dryRun && pending.Count > 0)
        {
            // Output trees (jpeg/webp/avif/jxl × full/thumb) must exist before the subprocess
            // `convert` writes into them.
            ImageConverter.EnsureDirectoriesCreated(assetRootPath, itemName);

            // Subprocess `convert` per variant: address-space isolation per encode. When
            // `convert` exits, the kernel reclaims 100% of the native codec buffers (libheif
            // reference frames, libaom / libsvtav1 / x265 lookahead, libjxl thread pools)
            // regardless of what the encoder retained internally. Trade-off is one fork/exec
            // + one PNG decode per variant (~30-80 ms each on Linux); on HDD-backed runs this
            // is dwarfed by disk wait anyway. The unit of parallelism is therefore one
            // VARIANT, not one file — the cap (--parallelism / Environment.ProcessorCount)
            // bounds total concurrent `convert` processes so CPU never oversubscribes either.
            var variants = pending
                          .SelectMany(p => ImageConverter.EnumerateVariants(p.Id, p.FilePath, p.Extension, itemName))
                          .ToList();

            int degree = parallelism <= 0 ? Environment.ProcessorCount : parallelism;
            if(degree > variants.Count) degree = variants.Count;

            Console.WriteLine(
                $"    Spinning up {degree} subprocess worker(s) for {variants.Count} variant(s) ({pending.Count} original(s) × 8)...");

            int totalVariants = variants.Count;
            int totalFiles    = pending.Count;
            var sw            = System.Diagnostics.Stopwatch.StartNew();

            bool        isTty       = !Console.IsOutputRedirected;
            int         lastDrawLen = 0;
            const int   barWidth    = 28;
            object      drawLock    = new();

            void Draw(bool finalDraw)
            {
                int doneVar  = Volatile.Read(ref convertedVariants) + Volatile.Read(ref failedVariants);
                double secs  = Math.Max(sw.Elapsed.TotalSeconds, 0.001);
                double varRate  = doneVar / secs;
                double origRate = varRate / 8.0;
                int    doneOrig = doneVar / 8;
                int    pct      = totalVariants == 0 ? 100 : (int)Math.Min(100, 100L * doneVar / totalVariants);
                int    filled   = totalVariants == 0 ? barWidth : (int)((long)barWidth * doneVar / totalVariants);
                if(filled > barWidth) filled = barWidth;
                string bar = "[" + new string('=', Math.Max(0, filled - 1)) +
                             (doneVar > 0 && filled < barWidth ? ">" : (filled == 0 ? "" : "=")) +
                             new string(' ', barWidth - filled) + "]";
                string etaStr;
                if(doneVar == 0 || doneVar >= totalVariants) etaStr = finalDraw ? "" : "ETA --";
                else
                {
                    double etaSec = (totalVariants - doneVar) / varRate;
                    etaStr = $"ETA {FormatDuration(etaSec)}";
                }
                string line =
                    $"    {bar} {doneOrig}/{totalFiles} orig ({doneVar}/{totalVariants} var) {pct,3}%  {origRate,5:0.00} orig/s  {etaStr}"
                       .TrimEnd();

                lock(drawLock)
                {
                    if(isTty)
                    {
                        Console.Write("\r" + line);
                        if(line.Length < lastDrawLen) Console.Write(new string(' ', lastDrawLen - line.Length));
                        lastDrawLen = line.Length;
                        if(finalDraw) Console.WriteLine();
                    }
                    else
                    {
                        Console.WriteLine(line);
                    }
                }
            }

            using var ticker = new Timer(_ => Draw(false), null, TimeSpan.FromMilliseconds(500),
                                         isTty ? TimeSpan.FromMilliseconds(500) : TimeSpan.FromSeconds(5));

            var po = new ParallelOptions { MaxDegreeOfParallelism = degree };
            Parallel.ForEach(variants, po, variant =>
            {
                bool   ok;
                string error;
                try
                {
                    (ok, error) = ImageConverter.ConvertOne(assetRootPath, variant);
                }
                catch(Exception ex)
                {
                    ok    = false;
                    error = $"unhandled exception: {ex.Message}";
                }

                if(ok)
                    Interlocked.Increment(ref convertedVariants);
                else
                {
                    Interlocked.Increment(ref failedVariants);
                    lock(drawLock)
                    {
                        if(isTty && lastDrawLen > 0)
                        {
                            Console.Write("\r" + new string(' ', lastDrawLen) + "\r");
                            lastDrawLen = 0;
                        }
                        string detail = string.IsNullOrEmpty(error) ? "" : $"  ({error})";
                        Console.WriteLine(
                            $"    \e[31mFAILED\e[0m  {variant.Id} {variant.OutputFormat} {variant.Resolution} {(variant.Thumbnail ? "thumb" : "full")}{detail}");
                    }
                }
            });

            ticker.Change(Timeout.Infinite, Timeout.Infinite);
            Draw(true);

            sw.Stop();
            double finalRate = sw.Elapsed.TotalSeconds > 0 ? totalFiles / sw.Elapsed.TotalSeconds : 0;
            Console.WriteLine(
                $"    Total elapsed {FormatDuration(sw.Elapsed.TotalSeconds)} — average {finalRate:0.00} orig/s");
        }

        // Variants are tracked individually; report at file granularity (8 variants per file).
        // "converted" counts files where ALL 8 variants succeeded; failed = the rest. Partial
        // files (1-7 variants done) are folded into the failed bucket because HasAllVariants
        // demands the full set, so a partial run is functionally equivalent to a fully-failed
        // one — the next pass picks it up regardless.
        int converted = convertedVariants / 8;
        int failed    = dryRun ? 0 : pending.Count - converted;

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

    /// <summary>Compact human-readable duration: <c>42s</c>, <c>3m17s</c>, <c>1h04m</c>.</summary>
    static string FormatDuration(double seconds)
    {
        if(double.IsNaN(seconds) || double.IsInfinity(seconds) || seconds < 0) return "--";
        var ts = TimeSpan.FromSeconds(seconds);
        if(ts.TotalHours   >= 1) return $"{(int)ts.TotalHours}h{ts.Minutes:00}m";
        if(ts.TotalMinutes >= 1) return $"{ts.Minutes}m{ts.Seconds:00}s";
        return $"{ts.TotalSeconds:0}s";
    }
}
