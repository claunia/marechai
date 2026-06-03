/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
//
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2003-2026 Natalia Portillo
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ImageMagick;

namespace Marechai.Helpers;

public class Photos
{
    public delegate Task ConversionFinished(bool result);

    public static void EnsureCreated(string assetRootPath, bool scan, string item)
    {
        List<string> paths = [];

        string photosRoot             = Path.Combine(assetRootPath,    scan ? "scans" : "photos");
        string itemPhotosRoot         = Path.Combine(photosRoot,     item);
        string itemThumbsRoot         = Path.Combine(itemPhotosRoot, "thumbs");
        string itemOriginalPhotosRoot = Path.Combine(itemPhotosRoot, "originals");

        paths.Add(photosRoot);
        paths.Add(itemPhotosRoot);
        paths.Add(itemThumbsRoot);
        paths.Add(itemOriginalPhotosRoot);

        paths.Add(Path.Combine(itemThumbsRoot, "jpeg", "4k"));
        paths.Add(Path.Combine(itemPhotosRoot, "jpeg", "4k"));

        paths.Add(Path.Combine(itemThumbsRoot, "webp", "4k"));
        paths.Add(Path.Combine(itemPhotosRoot, "webp", "4k"));

        paths.Add(Path.Combine(itemThumbsRoot, "avif", "4k"));
        paths.Add(Path.Combine(itemPhotosRoot, "avif", "4k"));

        paths.Add(Path.Combine(itemThumbsRoot, "jxl", "4k"));
        paths.Add(Path.Combine(itemPhotosRoot, "jxl", "4k"));

        foreach(string path in paths.Where(path => !Directory.Exists(path))) Directory.CreateDirectory(path);
    }

    public static void BackfillJxl(string assetRootPath, bool scan, string item)
    {
        string photosRoot     = Path.Combine(assetRootPath, scan ? "scans" : "photos");
        string itemPhotosRoot = Path.Combine(photosRoot,    item);
        string originalsRoot  = Path.Combine(itemPhotosRoot, "originals");

        if(!Directory.Exists(originalsRoot)) return;

        foreach(string originalFile in Directory.GetFiles(originalsRoot))
        {
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(originalFile);

            if(!Guid.TryParse(fileNameWithoutExt, out Guid id)) continue;

            string sourceFormat = Path.GetExtension(originalFile).TrimStart('.');

            // Check if JXL 4k thumbnail already exists — if so, assume all variants exist
            string checkPath = Path.Combine(itemPhotosRoot, "thumbs", "jxl", "4k", $"{id}.jxl");

            if(File.Exists(checkPath)) continue;

            Console.WriteLine("Backfilling JXL for {0}...", id);

            Convert(assetRootPath, id, originalFile, sourceFormat, "JXL", "4k", true,  scan, item);
            Convert(assetRootPath, id, originalFile, sourceFormat, "JXL", "4k", false, scan, item);
        }
    }

    public static bool Convert(string assetRootPath,  Guid   id,         string originalPath, string sourceFormat,
                               string outputFormat, string resolution, bool   thumbnail,    bool   scan, string item)
    {
        outputFormat = outputFormat.ToLowerInvariant();
        resolution   = resolution.ToLowerInvariant();
        sourceFormat = sourceFormat.ToLowerInvariant();

        string outputPath = Path.Combine(assetRootPath, scan ? "scans" : "photos", item);
        int    width, height;

        if(thumbnail) outputPath = Path.Combine(outputPath, "thumbs");

        outputPath = Path.Combine(outputPath, outputFormat);
        outputPath = Path.Combine(outputPath, resolution);

        switch(resolution)
        {
            case "4k":
                if(thumbnail)
                {
                    width  = 512;
                    height = 512;
                }
                else
                {
                    width  = 3840;
                    height = 2160;
                }

                break;
            default:
                return false;
        }

        string fileExt = outputFormat switch
        {
            "jpeg" => "jpg",
            "webp" => "webp",
            "avif" => "avif",
            "jxl"  => "jxl",
            _      => null
        };
        if(fileExt is null) return false;

        outputPath = Path.Combine(outputPath, $"{id}.{fileExt}");

        // Single in-process variant: decode source, resize, encode. Used by callers that
        // want one specific output (e.g. JXL backfill); the upload pipeline goes through
        // <see cref="ConversionWorker" /> instead which shares one decode across all 8 variants.
        EnsureMagickInitialized();
        try
        {
            using var img = new MagickImage(originalPath);
            img.Strip();
            Resize(img, width, height);
            WriteVariant(img, outputPath, outputFormat, thumbnail);
            return true;
        }
        catch(Exception)
        {
            return false;
        }
    }

    /// <summary>
    ///     Legacy entry point retained for source compatibility. Infers the encoder format from
    ///     <paramref name="outputPath" />'s extension and dispatches through the in-process
    ///     Magick.NET pipeline. Thumbnail-tier encoder tuning is applied when the path lives
    ///     under a <c>/thumbs/</c> segment.
    /// </summary>
    public static bool ConvertUsingImageMagick(string originalPath, string outputPath, int width, int height)
    {
        string ext = Path.GetExtension(outputPath).TrimStart('.').ToLowerInvariant();
        string outputFormat = ext switch
        {
            "jpg" or "jpeg" => "jpeg",
            "webp"          => "webp",
            "avif"          => "avif",
            "jxl"           => "jxl",
            _               => null
        };
        if(outputFormat is null) return false;

        bool thumbnail = outputPath.Replace('\\', '/').Contains("/thumbs/", StringComparison.OrdinalIgnoreCase);

        EnsureMagickInitialized();
        try
        {
            using var img = new MagickImage(originalPath);
            img.Strip();
            Resize(img, width, height);
            WriteVariant(img, outputPath, outputFormat, thumbnail);
            return true;
        }
        catch(Exception)
        {
            return false;
        }
    }

    /// <summary>
    ///     Real content-sniff via Magick.NET's <see cref="MagickImageInfo" /> (in-process, no
    ///     subprocess). Returns the canonical uppercase format (e.g. <c>"JPEG"</c>, <c>"PNG"</c>,
    ///     <c>"WEBP"</c>, <c>"AVIF"</c>, <c>"JXL"</c>, <c>"BMP"</c>, <c>"TIFF"</c>) plus
    ///     dimensions. <c>(null, 0, 0)</c> when the file is unreadable or unrecognised.
    /// </summary>
    public static (string format, int width, int height) Identify(string path)
    {
        EnsureMagickInitialized();
        try
        {
            var info = new MagickImageInfo(path);

            // MagickFormat enum names are PascalCase (Jpeg, Png, WebP, Avif, Jxl, Bmp, Tiff,
            // Tif). The existing admin-batch allow-lists compare against uppercase canonical
            // names ("JPEG", "TIFF", ...) so we upper-case and collapse the Tif/Tiff alias.
            string format = info.Format.ToString().ToUpperInvariant();
            if(format == "TIF") format = "TIFF";

            return (format, (int)info.Width, (int)info.Height);
        }
        catch(Exception)
        {
            return (null, 0, 0);
        }
    }

    /// <summary>
    ///     Generate a JPEG thumbnail constrained to <paramref name="maxWidth" />x
    ///     <paramref name="maxHeight" /> (downscale only, aspect preserved). Returns the
    ///     raw JPEG bytes or <c>null</c> on failure. Used by the admin batch-upload dialog
    ///     to render staging previews via an inline data URL.
    /// </summary>
    public static byte[] GenerateThumbnailJpeg(string srcPath, int maxWidth = 256, int maxHeight = 256)
    {
        EnsureMagickInitialized();
        try
        {
            using var img = new MagickImage(srcPath);
            img.Strip();
            Resize(img, maxWidth, maxHeight);
            img.Quality = 80;

            byte[] bytes = img.ToByteArray(MagickFormat.Jpeg);
            return bytes is { Length: > 0 } ? bytes : null;
        }
        catch(Exception)
        {
            return null;
        }
    }

    public void ConversionWorker(string assetRootPath, Guid id, string originalFilePath, string sourceFormat, bool scan,
                                 string item)
    {
        // In-process Magick.NET pipeline: decode the source ONCE and write all 8 variants
        // (4 formats \u00d7 thumb/full) from clones. Saves 7 PNG decode passes and 8
        // <c>convert</c> subprocess fork/exec cycles per upload vs the old per-variant
        // Process.Start path. Encodes run in parallel inside the upload so per-request
        // latency tracks the slowest single encoder (AVIF), not the sum of all 8.
        EnsureMagickInitialized();

        string photosOrScans = scan ? "scans" : "photos";

        // Pre-compute all 8 output paths so the parallel section is pure CPU work.
        var jobs = new List<(string Format, bool Thumbnail, string OutputPath, int Width, int Height)>(8);
        foreach((string format, string ext) in s_outputFormats)
        {
            string fullDir  = Path.Combine(assetRootPath, photosOrScans, item, format, "4k");
            string thumbDir = Path.Combine(assetRootPath, photosOrScans, item, "thumbs", format, "4k");
            jobs.Add((format, true,  Path.Combine(thumbDir, $"{id}.{ext}"), 512,  512));
            jobs.Add((format, false, Path.Combine(fullDir,  $"{id}.{ext}"), 3840, 2160));
        }

        MagickImage source;
        try
        {
            source = new MagickImage(originalFilePath);
            source.Strip();
        }
        catch(Exception)
        {
            // Mirror the legacy contract: invoke all per-variant events with `false` then the
            // aggregate event. No subscribers in the current tree but the API is public.
            InvokeVariantFailures();
            FinishedAll?.Invoke(false);
            return;
        }

        // Per-variant success bookkeeping so we can fire the matching event afterwards.
        var results   = new System.Collections.Concurrent.ConcurrentDictionary<(string, bool), bool>();
        var cloneLock = new object();

        try
        {
            // Parallelism inside a single upload \u2014 each clone is an independent encode.
            // ResourceLimits.Thread = 1 keeps OpenMP from oversubscribing CPUs if multiple
            // uploads land at once. The Clone() call itself is guarded because Magick.NET's
            // <see cref="IMagickImage{TQuantumType}" /> instances aren't documented as safe
            // for concurrent reads (clone touches internal profile/settings state); cloning
            // is fast (pixel-buffer memcpy) so the lock contention is negligible vs the
            // multi-100 ms encode that follows.
            Parallel.ForEach(jobs, job =>
            {
                IMagickImage<byte> clone;
                try
                {
                    lock(cloneLock) clone = source.Clone();
                }
                catch(Exception)
                {
                    results[(job.Format, job.Thumbnail)] = false;
                    return;
                }

                try
                {
                    Resize(clone, job.Width, job.Height);
                    WriteVariant(clone, job.OutputPath, job.Format, job.Thumbnail);
                    results[(job.Format, job.Thumbnail)] = true;
                }
                catch(Exception)
                {
                    results[(job.Format, job.Thumbnail)] = false;
                }
                finally
                {
                    clone.Dispose();
                }
            });
        }
        finally
        {
            source.Dispose();
        }

        // Fire per-variant events in a stable order, then the aggregate. None of these
        // currently have subscribers in the repo but the public surface is preserved.
        FinishedRenderingJpeg4kThumbnail?.Invoke(results.GetValueOrDefault(("jpeg", true)));
        FinishedRenderingJpeg4K?.Invoke(results.GetValueOrDefault(("jpeg",        false)));
        FinishedRenderingWebp4kThumbnail?.Invoke(results.GetValueOrDefault(("webp", true)));
        FinishedRenderingWebp4k?.Invoke(results.GetValueOrDefault(("webp",         false)));
        FinishedRenderingAvif4kThumbnail?.Invoke(results.GetValueOrDefault(("avif", true)));
        FinishedRenderingAvif4K?.Invoke(results.GetValueOrDefault(("avif",         false)));
        FinishedRenderingJxl4kThumbnail?.Invoke(results.GetValueOrDefault(("jxl", true)));
        FinishedRenderingJxl4K?.Invoke(results.GetValueOrDefault(("jxl",          false)));

        bool overall = results.Count == 8 && results.Values.All(v => v);
        FinishedAll?.Invoke(overall);

        void InvokeVariantFailures()
        {
            FinishedRenderingJpeg4kThumbnail?.Invoke(false);
            FinishedRenderingJpeg4K?.Invoke(false);
            FinishedRenderingWebp4kThumbnail?.Invoke(false);
            FinishedRenderingWebp4k?.Invoke(false);
            FinishedRenderingAvif4kThumbnail?.Invoke(false);
            FinishedRenderingAvif4K?.Invoke(false);
            FinishedRenderingJxl4kThumbnail?.Invoke(false);
            FinishedRenderingJxl4K?.Invoke(false);
        }
    }

    public event ConversionFinished FinishedAll;

    public event ConversionFinished FinishedRenderingJpeg4kThumbnail;
    public event ConversionFinished FinishedRenderingJpeg4K;
    public event ConversionFinished FinishedRenderingWebp4kThumbnail;
    public event ConversionFinished FinishedRenderingWebp4k;
    public event ConversionFinished FinishedRenderingAvif4kThumbnail;
    public event ConversionFinished FinishedRenderingAvif4K;
    public event ConversionFinished FinishedRenderingJxl4kThumbnail;
    public event ConversionFinished FinishedRenderingJxl4K;

    // ── In-process Magick.NET plumbing ────────────────────────────────────────

    /// <summary>Canonical output format list shared by single-variant <see cref="Convert" /> and bulk <see cref="ConversionWorker" />.</summary>
    static readonly (string Format, string Extension)[] s_outputFormats =
    {
        ("jpeg", "jpg"), ("webp", "webp"), ("avif", "avif"), ("jxl", "jxl")
    };

    /// <summary>
    ///     Shrink-only resize honouring ImageMagick's classic <c>{w}x{h}&gt;</c> behaviour
    ///     (never upscale, preserve aspect ratio). Magick.NET's <c>MagickGeometry { Greater = true }</c>
    ///     is the direct equivalent.
    /// </summary>
    static void Resize(IMagickImage<byte> img, int width, int height) =>
        img.Resize(new MagickGeometry((uint)width, (uint)height) { Greater = true });

    /// <summary>
    ///     Apply per-(format, thumbnail) encoder tuning matching the MobyGames conversion
    ///     pipeline (see <c>Marechai.MobyGames/Services/ImageConverter.cs</c>) and write
    ///     <paramref name="outputPath" />. Quality 75 full / 70 thumb; per-encoder effort knobs
    ///     biased towards speed for thumbnails (visible \u2264 512 px) and balanced for full-res.
    /// </summary>
    static void WriteVariant(IMagickImage<byte> img, string outputPath, string format, bool thumbnail)
    {
        (uint quality, string defineKey, string defineValue, MagickFormat mf) = format switch
        {
            "jpeg" => (thumbnail ? 70u : 75u, null,           null,            MagickFormat.Jpeg),
            // libwebp `method`: 0 = fastest, 6 = slowest/best. Default is 4.
            "webp" => (thumbnail ? 70u : 75u, "webp:method",  thumbnail ? "0" : "3", MagickFormat.WebP),
            // libheif/x265 `speed`: 1 = slowest/best, 9 = fastest. Default is 4.
            "avif" => (thumbnail ? 70u : 75u, "heic:speed",   thumbnail ? "9" : "7", MagickFormat.Avif),
            // libjxl `effort`: 1 = fastest, 9 = slowest/best. Default is 7.
            "jxl"  => (thumbnail ? 70u : 75u, "jxl:effort",   thumbnail ? "1" : "4", MagickFormat.Jxl),
            _      => (75u, null, null, MagickFormat.Unknown)
        };
        if(mf == MagickFormat.Unknown) throw new InvalidOperationException($"Unsupported output format '{format}'.");

        img.Quality = quality;
        if(defineKey is not null) img.Settings.SetDefine(defineKey, defineValue);
        img.Write(outputPath, mf);
    }

    /// <summary>
    ///     One-shot Magick.NET configuration. Pins per-process OpenMP threads to 1 so that
    ///     a burst of concurrent uploads doesn't catastrophically oversubscribe the CPU via
    ///     libheif/x265's internal pools. Idempotent and lock-free via <see cref="Interlocked.CompareExchange" />.
    /// </summary>
    static int s_magickInitialized;
    static void EnsureMagickInitialized()
    {
        if(Interlocked.CompareExchange(ref s_magickInitialized, 1, 0) != 0) return;
        try { ResourceLimits.Thread = 1; }
        catch { /* older versions may not expose Thread; harmless */ }
    }
}