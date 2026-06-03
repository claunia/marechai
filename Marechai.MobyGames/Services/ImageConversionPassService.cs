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

        int converted = 0;
        int failed    = 0;

        if(!dryRun && pending.Count > 0)
        {
            int degree = parallelism <= 0 ? Environment.ProcessorCount : parallelism;

            // Flatten every pending file into its 8 ImageMagick jobs (4 formats \u00d7 1 resolution
            // \u00d7 thumb/full). The Parallel.ForEach below caps total concurrent IM subprocesses
            // at `degree`, and each subprocess is pinned to 1 OpenMP thread via
            // MAGICK_THREAD_LIMIT=1 inside ImageConverter \u2014 so the worker pool is also the CPU pool,
            // no oversubscription.
            var queue = new List<(int FileIndex, Guid FileId, ImageConverter.Variant Variant)>(pending.Count * 8);
            var remainingPerFile = new System.Collections.Concurrent.ConcurrentDictionary<Guid, int>();
            var failedPerFile    = new System.Collections.Concurrent.ConcurrentDictionary<Guid, int>();

            for(int i = 0; i < pending.Count; i++)
            {
                var p = pending[i];
                int variantCount = 0;
                foreach(ImageConverter.Variant v in
                        ImageConverter.EnumerateVariants(p.Id, p.FilePath, p.Extension, itemName))
                {
                    queue.Add((i, p.Id, v));
                    variantCount++;
                }
                remainingPerFile[p.Id] = variantCount;
            }

            if(degree > queue.Count) degree = queue.Count;

            Console.WriteLine($"    Spinning up {degree} ImageMagick worker(s) for {queue.Count} variant(s) "
                            + $"across {pending.Count} original(s)...");

            int total = pending.Count;
            var sw    = System.Diagnostics.Stopwatch.StartNew();

            // Live progress bar repainted in-place via CR on a TTY. When stdout is redirected
            // (file / pipe / journald) we fall back to a periodic stat line every 5 s so logs
            // stay readable and the progress remains visible without smearing.
            bool        isTty       = !Console.IsOutputRedirected;
            int         lastDrawLen = 0;
            const int   barWidth    = 28;
            object      drawLock    = new();

            void Draw(bool finalDraw)
            {
                int done    = Volatile.Read(ref converted) + Volatile.Read(ref failed);
                double secs = Math.Max(sw.Elapsed.TotalSeconds, 0.001);
                double rate = done / secs;
                int    pct  = total == 0 ? 100 : (int)Math.Min(100, 100L * done / total);
                int    filled = total == 0 ? barWidth : (int)((long)barWidth * done / total);
                if(filled > barWidth) filled = barWidth;
                string bar = "[" + new string('=', Math.Max(0, filled - 1)) +
                             (done > 0 && filled < barWidth ? ">" : (filled == 0 ? "" : "=")) +
                             new string(' ', barWidth - filled) + "]";
                string etaStr;
                if(done == 0 || done >= total) etaStr = finalDraw ? "" : "ETA --";
                else
                {
                    double etaSec = (total - done) / rate;
                    etaStr = $"ETA {FormatDuration(etaSec)}";
                }
                string line = $"    {bar} {done}/{total} {pct,3}%  {rate,5:0.00} orig/s  {etaStr}".TrimEnd();

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
            Parallel.ForEach(queue, po, item =>
            {
                bool ok;
                try { ok = ImageConverter.ConvertOne(assetRootPath, item.Variant); }
                catch
                {
                    ok = false;
                }

                if(!ok)
                    failedPerFile.AddOrUpdate(item.FileId, 1, (_, n) => n + 1);

                // Decrement the file's remaining-variant counter. When it hits 0, the file is
                // fully processed \u2014 bump either the converted or failed bucket exactly once per
                // file so the progress bar stays in originals/sec terms.
                int left = remainingPerFile.AddOrUpdate(item.FileId, 0, (_, n) => n - 1);
                if(left == 0)
                {
                    if(failedPerFile.TryGetValue(item.FileId, out int f) && f > 0)
                    {
                        Interlocked.Increment(ref failed);
                        lock(drawLock)
                        {
                            if(isTty && lastDrawLen > 0)
                            {
                                Console.Write("\r" + new string(' ', lastDrawLen) + "\r");
                                lastDrawLen = 0;
                            }
                            Console.WriteLine($"    \e[31mFAILED\e[0m  {item.FileId}  ({f}/{8} variants failed)");
                        }
                    }
                    else
                    {
                        Interlocked.Increment(ref converted);
                    }
                }
            });

            // Stop the ticker and paint a final 100% line.
            ticker.Change(Timeout.Infinite, Timeout.Infinite);
            Draw(true);

            sw.Stop();
            double finalRate = sw.Elapsed.TotalSeconds > 0 ? total / sw.Elapsed.TotalSeconds : 0;
            Console.WriteLine($"    Total elapsed {FormatDuration(sw.Elapsed.TotalSeconds)} \u2014 average {finalRate:0.00} orig/s");
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
