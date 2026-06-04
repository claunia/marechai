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
        ("avif", "avif")
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

        foreach(string format in new[] { "jpeg", "webp", "avif" })
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
            (bool ok, string error) = ConvertOne(assetRootPath, v);
            if(!ok)
            {
                string detail = string.IsNullOrEmpty(error) ? "" : $"  ({error})";
                Console.WriteLine(
                    $"\e[33m    Warning: {v.OutputFormat} {v.Resolution} {(v.Thumbnail ? "thumbnail" : "full")} conversion failed{detail}\e[0m");
            }
        }
    }

    /// <summary>
    ///     One ImageMagick conversion job for a single (id, format, resolution, thumb/full) tuple.
    ///     Used as the unit of parallelism by the offline conversion pass so we can size the worker
    ///     pool to <see cref="Environment.ProcessorCount" /> and pin <c>MAGICK_THREAD_LIMIT=1</c>
    ///     per process — N parallel IM subprocesses == N CPU cores, no thread thrash.
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

    /// <summary>
    ///     Run ImageMagick for a single <see cref="Variant" />. Returns <c>(true, null)</c> on
    ///     success; on failure returns <c>(false, error)</c> where <c>error</c> is the trimmed
    ///     <c>convert</c> stderr (or a process-spawn exception message) so callers can surface
    ///     the actual cause instead of just "failed".
    /// </summary>
    public static (bool Ok, string Error) ConvertOne(string assetRootPath, Variant v) =>
        Convert(assetRootPath, v.Id, v.OriginalPath, v.SourceFormat, v.OutputFormat, v.Resolution, v.Thumbnail,
                v.ItemName);

    static (bool Ok, string Error) Convert(string assetRootPath, Guid id, string originalPath, string sourceFormat,
                                            string outputFormat, string resolution, bool thumbnail,
                                            string itemName = DefaultItemName)
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
                return (false, $"unsupported resolution '{resolution}'");
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

            default:
                return (false, $"unsupported output format '{outputFormat}'");
        }
    }

    /// <summary>
    ///     Per-(format, thumbnail) encoder knobs. Tuned for offline batch throughput on multi-core
    ///     hardware: thumbnails take the fastest preset since they're tiny anyway; full-resolution
    ///     outputs take a medium-fast preset that still gets reasonable compression. Quality is
    ///     lowered from the historical default of 80 because most assets here are 4k thumbnails for
    ///     a catalogue grid view \u2014 70-75 is visually indistinguishable from 80 at typical viewing
    ///     sizes and saves significant CPU on AVIF where higher quality means much slower encode.
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

            _ => (quality, null, null)
        };
    }

    static (bool Ok, string Error) ConvertUsingImageMagick(string originalPath, string outputPath, int width,
                                                            int height, string outputFormat, bool thumbnail)
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
                FileName               = "magick",
                CreateNoWindow         = true,
                RedirectStandardError  = true,
                RedirectStandardOutput = true
            }
        };

        // -limit thread 1 + MAGICK_THREAD_LIMIT=1 keep ImageMagick's own OpenMP core
        // single-threaded so the only multi-threaded codec is libheif/x265 (AVIF),
        // which has its own pool we let breathe.
        //
        // IMv7 `magick` is strict about argument order: operations like -resize act on
        // images already on the stack. Input MUST come before any operation, otherwise
        // the operation fails with "no images found". -limit is a global setting and can
        // precede the input. -define is per-encoder and must precede the output to take
        // effect, so we place it between input and -resize.
        convert.StartInfo.ArgumentList.Add("-limit");
        convert.StartInfo.ArgumentList.Add("thread");
        convert.StartInfo.ArgumentList.Add("1");

        // [0] restricts multi-frame containers (animated GIF, multipage TIFF, PDF, ICO with
        // varying sizes) to the first frame. AVIF/WebP would silently encode only the first
        // frame anyway — making it explicit here keeps output consistent across encoders.
        convert.StartInfo.ArgumentList.Add($"{originalPath}[0]");

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
        convert.StartInfo.ArgumentList.Add(outputPath);

        convert.StartInfo.Environment["MAGICK_THREAD_LIMIT"] = "1";

        try
        {
            convert.Start();

            // Read both pipes concurrently. If either pipe fills its kernel buffer (typical:
            // 64 KB) the child blocks on write and the whole subprocess deadlocks against our
            // WaitForExit. Async reads drain both in parallel and let large stderr text through.
            Task<string> stderrTask = convert.StandardError.ReadToEndAsync();
            Task<string> stdoutTask = convert.StandardOutput.ReadToEndAsync();
            convert.WaitForExit();
            string stderr = stderrTask.GetAwaiter().GetResult();
            _ = stdoutTask.GetAwaiter().GetResult();

            if(convert.ExitCode == 0) return (true, null);

            // Trim + collapse to a single line + truncate so a multi-megabyte ImageMagick
            // dump doesn't blow up the progress display.
            string detail = (stderr ?? "").Trim().Replace('\r', ' ').Replace('\n', ' ');
            if(detail.Length > 400) detail = detail[..400] + "…";
            if(detail.Length == 0)  detail = "(no stderr)";

            return (false, $"exit {convert.ExitCode}: {detail}");
        }
        catch(Exception ex)
        {
            return (false, $"process spawn failed: {ex.Message}");
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
