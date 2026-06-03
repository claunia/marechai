using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Marechai.MobyGames.Services;

public static class ImageConverter
{
    const string DefaultItemName = "software-covers";

    // Keep these synchronised with the `formats` / `resolutions` arrays in ConvertAll
    // and with the output-path layout in Convert(...). The conversion pass uses these
    // to check, purely from the filesystem, whether all expected outputs exist for a
    // given (id, itemName) pair without consulting any database.
    static readonly (string Format, string Extension)[] OutputFormats =
    {
        ("jpeg", "jpg"),
        ("webp", "webp"),
        ("avif", "avif"),
        ("jxl",  "jxl")
    };

    static readonly string[] OutputResolutions = { "4k" };

    public static void EnsureDirectoriesCreated(string assetRootPath) =>
        EnsureDirectoriesCreated(assetRootPath, DefaultItemName);

    public static void EnsureDirectoriesCreated(string assetRootPath, string itemName)
    {
        List<string> paths = [];

        string photosRoot             = Path.Combine(assetRootPath, "photos");
        string itemPhotosRoot         = Path.Combine(photosRoot,    itemName);
        string itemThumbsRoot         = Path.Combine(itemPhotosRoot, "thumbs");
        string itemOriginalPhotosRoot = Path.Combine(itemPhotosRoot, "originals");

        paths.Add(photosRoot);
        paths.Add(itemPhotosRoot);
        paths.Add(itemThumbsRoot);
        paths.Add(itemOriginalPhotosRoot);

        foreach(string format in new[] { "jpeg", "webp", "avif", "jxl" })
        {
            foreach(string resolution in new[] { "4k" })
            {
                paths.Add(Path.Combine(itemThumbsRoot, format, resolution));
                paths.Add(Path.Combine(itemPhotosRoot, format, resolution));
            }
        }

        foreach(string path in paths.Where(path => !Directory.Exists(path))) Directory.CreateDirectory(path);
    }

    public static void ConvertAll(string assetRootPath, Guid id, string originalFilePath, string sourceFormat) =>
        ConvertAll(assetRootPath, id, originalFilePath, sourceFormat, DefaultItemName);

    public static void ConvertAll(string assetRootPath, Guid id, string originalFilePath, string sourceFormat,
                                  string itemName)
    {
        // Sequential walk through every variant. Callers that want concurrency should drive the
        // flat-queue path directly (see <see cref="EnumerateVariants" /> + <see cref="ConvertOne" />),
        // which is what <c>ImageConversionPassService</c> does so the whole pass shares a single
        // <see cref="System.Threading.Tasks.Parallel.ForEach" /> with one ImageMagick subprocess per
        // CPU core. The old <c>Task.WaitAll</c> shape exploded into 8 IM subprocesses per outer
        // worker on a multi-file pass which catastrophically oversubscribed CPU.
        foreach(Variant v in EnumerateVariants(id, originalFilePath, sourceFormat, itemName))
        {
            if(!ConvertOne(assetRootPath, v))
                Console.WriteLine($"\e[33m    Warning: {v.OutputFormat} {v.Resolution} {(v.Thumbnail ? "thumbnail" : "full")} conversion failed\e[0m");
        }
    }

    /// <summary>
    ///     One ImageMagick conversion job for a single (id, format, resolution, thumb/full) tuple.
    ///     Used as the unit of parallelism by the offline conversion pass so we can size the worker
    ///     pool to <see cref="Environment.ProcessorCount" /> and pin <c>MAGICK_THREAD_LIMIT=1</c>
    ///     per process \u2014 N parallel IM subprocesses == N CPU cores, no thread thrash.
    /// </summary>
    public readonly record struct Variant(Guid Id, string OriginalPath, string SourceFormat,
                                          string OutputFormat, string Resolution, bool Thumbnail, string ItemName);

    /// <summary>Expand the canonical output set (4 formats \u00d7 1 resolution \u00d7 thumb/full = 8) for one source file.</summary>
    public static IEnumerable<Variant> EnumerateVariants(Guid id, string originalFilePath, string sourceFormat,
                                                         string itemName = DefaultItemName)
    {
        foreach((string format, string _) in OutputFormats)
        foreach(string resolution in OutputResolutions)
        {
            yield return new Variant(id, originalFilePath, sourceFormat, format, resolution, true,  itemName);
            yield return new Variant(id, originalFilePath, sourceFormat, format, resolution, false, itemName);
        }
    }

    /// <summary>Run ImageMagick for a single <see cref="Variant" />. Returns <c>true</c> on success.</summary>
    public static bool ConvertOne(string assetRootPath, Variant v) =>
        Convert(assetRootPath, v.Id, v.OriginalPath, v.SourceFormat, v.OutputFormat, v.Resolution, v.Thumbnail,
                v.ItemName);

    /// <summary>
    ///     In-process variant of the per-file conversion. Decodes the source ONCE via Magick.NET
    ///     and writes all 8 outputs (4 formats \u00d7 thumb/full) from the same in-memory pixel buffer.
    ///     Equivalent to running <see cref="ConvertOne" /> 8 times but skips:
    ///     <list type="bullet">
    ///         <item>7 of the 8 PNG decode passes (\u223c50\u2013500 ms each on 4k sources).</item>
    ///         <item>8 <c>convert</c> subprocess fork/exec/init cycles (\u223c30\u201380 ms each on Linux).</item>
    ///     </list>
    ///     Used by <see cref="ImageConversionPassService" /> as the unit of parallelism so the
    ///     outer <see cref="System.Threading.Tasks.Parallel.ForEach" /> caps total concurrent
    ///     decoders at <see cref="Environment.ProcessorCount" /> and the saved decode work shows
    ///     up directly in originals/sec throughput.
    /// </summary>
    /// <returns>The number of variants that failed (0 = full success).</returns>
    public static int ConvertFileInProcess(string assetRootPath, Guid id, string originalFilePath,
                                           string sourceFormat, string itemName = DefaultItemName)
    {
        EnsureMagickInitialized();
        int failed = 0;

        // Decode the source once. Strip metadata; both thumb and full inherit the cleared profile.
        ImageMagick.MagickImage source;
        try
        {
            source = new ImageMagick.MagickImage(originalFilePath);
            source.Strip();
        }
        catch(Exception ex)
        {
            Console.WriteLine($"\e[31m    Failed to decode {originalFilePath}: {ex.Message}\e[0m");
            // Mirror the subprocess path's failure granularity: all 8 variants are considered failed.
            return OutputFormats.Length * OutputResolutions.Length * 2;
        }

        try
        {
            // ── Full-resolution branch ─────────────────────────────────────────────
            foreach(string resolution in OutputResolutions)
            {
                (int fw, int fh) = ResolutionFor(resolution, thumbnail: false);
                if(fw == 0) { failed += OutputFormats.Length; continue; }

                // Clone so the source stays at native resolution for the thumbnail branch.
                using var full = source.Clone();
                Resize(full, fw, fh);

                foreach((string format, string ext) in OutputFormats)
                {
                    string outputPath = Path.Combine(assetRootPath, "photos", itemName, format, resolution,
                                                     $"{id}.{ext}");
                    if(!WriteVariant(full, outputPath, format, thumbnail: false)) failed++;
                }
            }

            // ── Thumbnail branch ───────────────────────────────────────────────────
            foreach(string resolution in OutputResolutions)
            {
                (int tw, int th) = ResolutionFor(resolution, thumbnail: true);
                if(tw == 0) { failed += OutputFormats.Length; continue; }

                using var thumb = source.Clone();
                Resize(thumb, tw, th);

                foreach((string format, string ext) in OutputFormats)
                {
                    string outputPath = Path.Combine(assetRootPath, "photos", itemName, "thumbs", format, resolution,
                                                     $"{id}.{ext}");
                    if(!WriteVariant(thumb, outputPath, format, thumbnail: true)) failed++;
                }
            }
        }
        finally
        {
            source.Dispose();
        }

        return failed;
    }

    static (int Width, int Height) ResolutionFor(string resolution, bool thumbnail)
    {
        return resolution.ToLowerInvariant() switch
        {
            "4k" => thumbnail ? (512, 512) : (3840, 2160),
            _    => (0, 0)
        };
    }

    /// <summary>
    ///     Resize honouring ImageMagick's classic <c>{w}x{h}&gt;</c> behaviour: shrink only, never
    ///     upscale, keep aspect ratio. Magick.NET's <c>MagickGeometry { Greater = true }</c> maps
    ///     directly to that.
    /// </summary>
    static void Resize(ImageMagick.IMagickImage<byte> img, int width, int height) =>
        img.Resize(new ImageMagick.MagickGeometry((uint)width, (uint)height) { Greater = true });

    /// <summary>
    ///     Apply per-(format, thumbnail) quality + encoder-effort tuning and write the file.
    ///     Matches the knobs of the subprocess path exactly so output sizes / quality are
    ///     interchangeable between the two backends.
    /// </summary>
    static bool WriteVariant(ImageMagick.IMagickImage<byte> img, string outputPath, string format, bool thumbnail)
    {
        (int quality, string defineKey, string defineValue) = GetEncoderTuning(format, thumbnail);

        try
        {
            img.Quality = (uint)quality;

            // Apply per-encoder define (heic:speed, jxl:effort, webp:method).
            if(defineKey is not null)
                img.Settings.SetDefine(defineKey, defineValue);

            ImageMagick.MagickFormat mf = format switch
            {
                "jpeg" => ImageMagick.MagickFormat.Jpeg,
                "webp" => ImageMagick.MagickFormat.WebP,
                "avif" => ImageMagick.MagickFormat.Avif,
                "jxl"  => ImageMagick.MagickFormat.Jxl,
                _      => ImageMagick.MagickFormat.Unknown
            };
            if(mf == ImageMagick.MagickFormat.Unknown) return false;

            img.Write(outputPath, mf);
            return true;
        }
        catch(Exception ex)
        {
            Console.WriteLine($"\e[33m    {format} {(thumbnail ? "thumb" : "full")} encode failed for {outputPath}: {ex.Message}\e[0m");
            return false;
        }
    }

    /// <summary>
    ///     One-shot Magick.NET configuration: pin to a single core's worth of OpenMP threads so the
    ///     N parallel workers in <see cref="ImageConversionPassService" /> map cleanly onto N CPUs.
    ///     Called lazily on first <see cref="ConvertFileInProcess" /> invocation; idempotent.
    /// </summary>
    static int s_magickInitialized;
    static void EnsureMagickInitialized()
    {
        if(System.Threading.Interlocked.CompareExchange(ref s_magickInitialized, 1, 0) != 0) return;
        try
        {
            // Magick.NET's per-process thread limit. AVIF (libheif/x265) and JXL ignore this and
            // run their own pool; we let them since we already proved over-pinning hurts more than it
            // helps in this workload (taskset experiment).
            ImageMagick.ResourceLimits.Thread = 1;
        }
        catch { /* older versions may not expose Thread; harmless */ }
    }

    static bool Convert(string assetRootPath, Guid id, string originalPath, string sourceFormat,
                        string outputFormat,  string resolution, bool thumbnail, string itemName = DefaultItemName)
    {
        outputFormat = outputFormat.ToLowerInvariant();
        resolution   = resolution.ToLowerInvariant();
        sourceFormat = sourceFormat.ToLowerInvariant();

        string outputPath = Path.Combine(assetRootPath, "photos", itemName);

        int width, height;

        if(thumbnail) outputPath = Path.Combine(outputPath, "thumbs");

        outputPath = Path.Combine(outputPath, outputFormat);
        outputPath = Path.Combine(outputPath, resolution);

        switch(resolution)
        {
            case "4k":
                if(thumbnail) { width = 512;  height = 512; }
                else          { width = 3840; height = 2160; }

                break;
            default:
                return false;
        }

        switch(outputFormat)
        {
            case "jpeg":
                outputPath = Path.Combine(outputPath, $"{id}.jpg");

                return ConvertUsingImageMagick(originalPath, outputPath, width, height, "jpeg", thumbnail);

            case "webp":
                outputPath = Path.Combine(outputPath, $"{id}.webp");

                return ConvertUsingImageMagick(originalPath, outputPath, width, height, "webp", thumbnail);

            case "avif":
                outputPath = Path.Combine(outputPath, $"{id}.avif");

                return ConvertUsingImageMagick(originalPath, outputPath, width, height, "avif", thumbnail);

            case "jxl":
                outputPath = Path.Combine(outputPath, $"{id}.jxl");

                return ConvertUsingImageMagick(originalPath, outputPath, width, height, "jxl", thumbnail);

            default:
                return false;
        }
    }

    /// <summary>
    ///     Per-(format, thumbnail) encoder knobs. Tuned for offline batch throughput on multi-core
    ///     hardware: thumbnails take the fastest preset since they're tiny anyway; full-resolution
    ///     outputs take a medium-fast preset that still gets reasonable compression. Quality is
    ///     lowered from the historical default of 80 because most assets here are 4k thumbnails for
    ///     a catalogue grid view \u2014 70-75 is visually indistinguishable from 80 at typical viewing
    ///     sizes and saves significant CPU on AVIF/JXL where higher quality means much slower encode.
    /// </summary>
    static (int Quality, string DefineKey, string DefineValue) GetEncoderTuning(string format, bool thumbnail)
    {
        int quality = thumbnail ? 70 : 75;

        return (format, thumbnail) switch
        {
            // JPEG has nothing beyond -quality; libjpeg-turbo is already fast.
            ("jpeg", _)   => (quality, null,           null),

            // libwebp `method`: 0 = fastest, 6 = slowest/best. Default is 4.
            ("webp", true)  => (quality, "webp:method", "0"),
            ("webp", false) => (quality, "webp:method", "3"),

            // libheif/x265 `speed`: 1 = slowest/best, 9 = fastest. Default is 4.
            // Thumbnails go full-speed; full-res uses a moderately fast preset (7).
            ("avif", true)  => (quality, "heic:speed", "9"),
            ("avif", false) => (quality, "heic:speed", "7"),

            // libjxl `effort`: 1 = fastest, 9 = slowest/best. Default is 7.
            // Thumbnails at 1 (lightning), full-res at 4 (decent compression, ~half default CPU).
            ("jxl", true)  => (quality, "jxl:effort", "1"),
            ("jxl", false) => (quality, "jxl:effort", "4"),

            _ => (quality, null, null)
        };
    }

    static bool ConvertUsingImageMagick(string originalPath, string outputPath, int width, int height,
                                        string outputFormat, bool thumbnail)
    {
        // We deliberately do NOT wrap the spawn in `taskset -c <n>`. Empirically, pinning each
        // convert to a single CPU cuts AVIF (libheif → x265) throughput by ~4×, which dominates
        // total wall time. At the parallelism levels we use here the natural x265 thread pool
        // (~4 cores per AVIF) plus the other 7 single-threaded variants for sibling files
        // already saturate all available cores cleanly. The CPU ceiling for the full 8-variant
        // set is roughly `Environment.ProcessorCount / per-file-CPU-cost` originals/sec.
        (int quality, string defineKey, string defineValue) = GetEncoderTuning(outputFormat, thumbnail);

        var convert = new Process
        {
            StartInfo =
            {
                FileName               = "convert",
                CreateNoWindow         = true,
                RedirectStandardError  = true,
                RedirectStandardOutput = true
            }
        };

        // -limit thread 1 + MAGICK_THREAD_LIMIT=1 keep ImageMagick's own OpenMP core
        // single-threaded so the only multi-threaded codec is libheif/x265 (AVIF),
        // which has its own pool we let breathe.
        convert.StartInfo.ArgumentList.Add("-limit");
        convert.StartInfo.ArgumentList.Add("thread");
        convert.StartInfo.ArgumentList.Add("1");

        // Per-encoder speed/effort knob (skip for JPEG).
        if(defineKey is not null)
        {
            convert.StartInfo.ArgumentList.Add("-define");
            convert.StartInfo.ArgumentList.Add($"{defineKey}={defineValue}");
        }

        convert.StartInfo.ArgumentList.Add("-resize");
        convert.StartInfo.ArgumentList.Add($"{width}x{height}>");
        convert.StartInfo.ArgumentList.Add("-strip");
        convert.StartInfo.ArgumentList.Add("-quality");
        convert.StartInfo.ArgumentList.Add(quality.ToString());
        convert.StartInfo.ArgumentList.Add(originalPath);
        convert.StartInfo.ArgumentList.Add(outputPath);

        convert.StartInfo.Environment["MAGICK_THREAD_LIMIT"] = "1";

        try
        {
            convert.Start();
            convert.StandardOutput.ReadToEnd();
            convert.WaitForExit();

            return convert.ExitCode == 0;
        }
        catch(Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Returns the canonical originals path for an image of the given id under the named
    /// item directory: <c>{assetRootPath}/photos/{itemName}/originals/{id}.{extension}</c>.
    /// Used by the offline conversion pass to locate source files without any database
    /// access.
    /// </summary>
    public static string GetOriginalPath(string assetRootPath, Guid id, string extension,
                                         string itemName = DefaultItemName)
    {
        string ext = (extension ?? "").TrimStart('.').ToLowerInvariant();

        return Path.Combine(assetRootPath, "photos", itemName, "originals", $"{id}.{ext}");
    }

    /// <summary>
    /// Returns <c>true</c> only when EVERY converted output for <paramref name="id"/> exists
    /// on disk under <paramref name="assetRootPath"/>/photos/<paramref name="itemName"/>/.
    /// Used by the offline conversion pass to skip images that have already been fully
    /// converted. The check is filesystem-only — no database access.
    /// </summary>
    public static bool HasAllVariants(string assetRootPath, Guid id, string itemName = DefaultItemName)
    {
        string photosRoot = Path.Combine(assetRootPath, "photos", itemName);

        foreach((string format, string ext) in OutputFormats)
        {
            foreach(string resolution in OutputResolutions)
            {
                string thumbPath = Path.Combine(photosRoot, "thumbs", format, resolution, $"{id}.{ext}");
                string fullPath  = Path.Combine(photosRoot, format, resolution, $"{id}.{ext}");

                if(!File.Exists(thumbPath) || !File.Exists(fullPath))
                    return false;
            }
        }

        return true;
    }

}
