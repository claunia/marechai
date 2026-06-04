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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

    /// <summary>
    ///     Convert one variant by inferring the encoder format from <paramref name="outputPath" />'s
    ///     extension. Thumbnail-tier encoder tuning is applied when the path lives under a
    ///     <c>/thumbs/</c> segment.
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

        return ConvertVariant(originalPath, outputPath, width, height, outputFormat, thumbnail);
    }

    /// <summary>
    ///     Content-sniff via the ImageMagick <c>identify</c> CLI. Returns the canonical uppercase
    ///     format (e.g. <c>"JPEG"</c>, <c>"PNG"</c>, <c>"WEBP"</c>, <c>"AVIF"</c>, <c>"JXL"</c>,
    ///     <c>"BMP"</c>, <c>"TIFF"</c>) plus dimensions. <c>(null, 0, 0)</c> when the file is
    ///     unreadable or unrecognised. The <c>[0]</c> frame selector restricts multi-frame
    ///     containers (TIFF, GIF, PDF, ICO) to the first page so the output is a single line.
    /// </summary>
    public static (string format, int width, int height) Identify(string path)
    {
        try
        {
            var p = new Process
            {
                StartInfo =
                {
                    FileName               = "identify",
                    CreateNoWindow         = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError  = true
                }
            };
            p.StartInfo.ArgumentList.Add("-format");
            p.StartInfo.ArgumentList.Add("%m %w %h\n");
            p.StartInfo.ArgumentList.Add($"{path}[0]");
            p.StartInfo.Environment["MAGICK_THREAD_LIMIT"] = "1";

            p.Start();
            string stdout = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            if(p.ExitCode != 0) return (null, 0, 0);

            string firstLine = stdout.Split('\n', 2)[0].Trim();
            string[] parts   = firstLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if(parts.Length < 3) return (null, 0, 0);
            if(!int.TryParse(parts[1], out int w) || !int.TryParse(parts[2], out int h)) return (null, 0, 0);

            // Existing admin-batch allow-lists compare against uppercase canonical names
            // ("JPEG", "TIFF", ...); collapse the Tif/Tiff alias to match.
            string format = parts[0].ToUpperInvariant();
            if(format == "TIF") format = "TIFF";

            return (format, w, h);
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
        try
        {
            var p = new Process
            {
                StartInfo =
                {
                    FileName               = "convert",
                    CreateNoWindow         = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError  = true
                }
            };
            p.StartInfo.ArgumentList.Add("-limit");
            p.StartInfo.ArgumentList.Add("thread");
            p.StartInfo.ArgumentList.Add("1");
            p.StartInfo.ArgumentList.Add($"{srcPath}[0]");
            p.StartInfo.ArgumentList.Add("-resize");
            p.StartInfo.ArgumentList.Add($"{maxWidth}x{maxHeight}>");
            p.StartInfo.ArgumentList.Add("-strip");
            p.StartInfo.ArgumentList.Add("-quality");
            p.StartInfo.ArgumentList.Add("80");
            // `jpeg:-` writes the encoded JPEG to stdout so we don't need a temp file.
            p.StartInfo.ArgumentList.Add("jpeg:-");
            p.StartInfo.Environment["MAGICK_THREAD_LIMIT"] = "1";

            p.Start();
            using var ms = new MemoryStream();
            p.StandardOutput.BaseStream.CopyTo(ms);
            p.WaitForExit();

            if(p.ExitCode != 0) return null;

            byte[] bytes = ms.ToArray();
            return bytes.Length > 0 ? bytes : null;
        }
        catch(Exception)
        {
            return null;
        }
    }

    public void ConversionWorker(string assetRootPath, Guid id, string originalFilePath, string sourceFormat, bool scan,
                                 string item)
    {
        string photosOrScans = scan ? "scans" : "photos";

        // Pre-compute all 8 output paths so the parallel section is pure subprocess invocation.
        var jobs = new List<(string Format, bool Thumbnail, string OutputPath, int Width, int Height)>(8);
        foreach((string format, string ext) in s_outputFormats)
        {
            string fullDir  = Path.Combine(assetRootPath, photosOrScans, item, format, "4k");
            string thumbDir = Path.Combine(assetRootPath, photosOrScans, item, "thumbs", format, "4k");
            jobs.Add((format, true,  Path.Combine(thumbDir, $"{id}.{ext}"), 512,  512));
            jobs.Add((format, false, Path.Combine(fullDir,  $"{id}.{ext}"), 3840, 2160));
        }

        // Subprocess `convert` per variant: 8 fork/exec per upload. Each subprocess gets a
        // fresh address space, so any native arena retention in libheif / libaom / libjxl /
        // libsvtav1 is reclaimed by the kernel on exit — critical for a long-running server
        // that handles thousands of uploads before restart. The fork/exec overhead is
        // ~30-80 ms per variant; concurrency is bounded by whatever Parallel.ForEach picks
        // from the thread pool (typically Environment.ProcessorCount).
        var results = new System.Collections.Concurrent.ConcurrentDictionary<(string, bool), bool>();

        Parallel.ForEach(jobs, job =>
        {
            bool ok = ConvertVariant(originalFilePath, job.OutputPath, job.Width, job.Height, job.Format,
                                     job.Thumbnail);
            results[(job.Format, job.Thumbnail)] = ok;
        });

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

    // ── Subprocess `convert` plumbing ─────────────────────────────────────────

    /// <summary>Canonical output format list shared by single-variant <see cref="Convert" /> and bulk <see cref="ConversionWorker" />.</summary>
    static readonly (string Format, string Extension)[] s_outputFormats =
    {
        ("jpeg", "jpg"), ("webp", "webp"), ("avif", "avif"), ("jxl", "jxl")
    };

    /// <summary>
    ///     Per-(format, thumbnail) encoder tuning. Mirrors the MobyGames conversion pipeline
    ///     (<c>Marechai.MobyGames/Services/ImageConverter.cs</c>): quality 75 full / 70 thumb;
    ///     per-encoder effort knobs biased towards speed for thumbnails (visible up to 512 px)
    ///     and balanced for full-res. Returns <c>(quality, defineKey, defineValue)</c>;
    ///     <c>defineKey</c> is <c>null</c> for JPEG (libjpeg-turbo has nothing beyond
    ///     <c>-quality</c>).
    /// </summary>
    static (int Quality, string DefineKey, string DefineValue) GetEncoderTuning(string format, bool thumbnail)
    {
        int quality = thumbnail ? 70 : 75;

        return (format, thumbnail) switch
        {
            ("jpeg", _)     => (quality, null,           null),
            // libwebp `method`: 0 = fastest, 6 = slowest/best. Default is 4.
            ("webp", true)  => (quality, "webp:method", "0"),
            ("webp", false) => (quality, "webp:method", "3"),
            // libheif `speed`: 1 = slowest/best, 9 = fastest. Default is 4. Underneath libheif
            // is libaom / libsvtav1 / x265 depending on the IM build.
            ("avif", true)  => (quality, "heic:speed",  "9"),
            ("avif", false) => (quality, "heic:speed",  "7"),
            // libjxl `effort`: 1 = fastest, 9 = slowest/best. Default is 7.
            ("jxl",  true)  => (quality, "jxl:effort",  "1"),
            ("jxl",  false) => (quality, "jxl:effort",  "4"),
            _               => (quality, null, null)
        };
    }

    /// <summary>
    ///     Run one ImageMagick <c>convert</c> subprocess for a single (originalPath, outputPath)
    ///     transcode with the per-format tuning from <see cref="GetEncoderTuning" />. Returns
    ///     <c>true</c> on a clean exit (status 0), <c>false</c> on any failure. Pixel data
    ///     never enters the .NET process — stdin/stdout are not used and the encoder runs
    ///     entirely in the subprocess's address space, so any native arena retention is
    ///     reclaimed by the kernel on exit.
    /// </summary>
    static bool ConvertVariant(string originalPath, string outputPath, int width, int height,
                               string outputFormat, bool thumbnail)
    {
        (int quality, string defineKey, string defineValue) = GetEncoderTuning(outputFormat, thumbnail);

        try
        {
            var p = new Process
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
            // single-threaded so the only multi-threaded codec is whichever AVIF backend
            // libheif is wired to (libaom / libsvtav1 / x265), which has its own pool that we
            // let breathe — capping it costs more throughput than it saves.
            p.StartInfo.ArgumentList.Add("-limit");
            p.StartInfo.ArgumentList.Add("thread");
            p.StartInfo.ArgumentList.Add("1");

            // Per-encoder speed/effort knob (skip for JPEG).
            if(defineKey is not null)
            {
                p.StartInfo.ArgumentList.Add("-define");
                p.StartInfo.ArgumentList.Add($"{defineKey}={defineValue}");
            }

            p.StartInfo.ArgumentList.Add("-resize");
            p.StartInfo.ArgumentList.Add($"{width}x{height}>");
            p.StartInfo.ArgumentList.Add("-strip");
            p.StartInfo.ArgumentList.Add("-quality");
            p.StartInfo.ArgumentList.Add(quality.ToString());
            // [0] selects the first frame for multi-frame containers (TIFF, GIF, PDF, ICO).
            p.StartInfo.ArgumentList.Add($"{originalPath}[0]");
            p.StartInfo.ArgumentList.Add(outputPath);

            p.StartInfo.Environment["MAGICK_THREAD_LIMIT"] = "1";

            p.Start();

            // Read both pipes concurrently. If either pipe fills its kernel buffer (typical:
            // 64 KB) the child blocks on write and the whole subprocess deadlocks against our
            // WaitForExit. Async reads drain both in parallel and let large stderr text through.
            Task<string> stderrTask = p.StandardError.ReadToEndAsync();
            Task<string> stdoutTask = p.StandardOutput.ReadToEndAsync();
            p.WaitForExit();
            string stderr = stderrTask.GetAwaiter().GetResult();
            _ = stdoutTask.GetAwaiter().GetResult();

            if(p.ExitCode == 0) return true;

            // Log the actual `convert` complaint so operators can tell apart "bad source
            // file", "encoder doesn't like the colorspace", "disk full", etc. instead of
            // just seeing a silent false.
            string detail = (stderr ?? "").Trim().Replace('\r', ' ').Replace('\n', ' ');
            if(detail.Length > 400) detail = detail[..400] + "…";
            if(detail.Length == 0)  detail = "(no stderr)";
            Console.Error.WriteLine(
                $"convert failed for {outputFormat} {(thumbnail ? "thumb" : "full")} {outputPath}: exit {p.ExitCode}: {detail}");
            return false;
        }
        catch(Exception ex)
        {
            Console.Error.WriteLine($"convert spawn failed for {outputPath}: {ex.Message}");
            return false;
        }
    }
}