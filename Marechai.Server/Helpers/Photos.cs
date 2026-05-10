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

        paths.Add(Path.Combine(itemThumbsRoot, "jpeg", "hd"));
        paths.Add(Path.Combine(itemThumbsRoot, "jpeg", "1440p"));
        paths.Add(Path.Combine(itemThumbsRoot, "jpeg", "4k"));
        paths.Add(Path.Combine(itemPhotosRoot, "jpeg", "hd"));
        paths.Add(Path.Combine(itemPhotosRoot, "jpeg", "1440p"));
        paths.Add(Path.Combine(itemPhotosRoot, "jpeg", "4k"));

        paths.Add(Path.Combine(itemThumbsRoot, "webp", "hd"));
        paths.Add(Path.Combine(itemThumbsRoot, "webp", "1440p"));
        paths.Add(Path.Combine(itemThumbsRoot, "webp", "4k"));
        paths.Add(Path.Combine(itemPhotosRoot, "webp", "hd"));
        paths.Add(Path.Combine(itemPhotosRoot, "webp", "1440p"));
        paths.Add(Path.Combine(itemPhotosRoot, "webp", "4k"));

        paths.Add(Path.Combine(itemThumbsRoot, "avif", "hd"));
        paths.Add(Path.Combine(itemThumbsRoot, "avif", "1440p"));
        paths.Add(Path.Combine(itemThumbsRoot, "avif", "4k"));
        paths.Add(Path.Combine(itemPhotosRoot, "avif", "hd"));
        paths.Add(Path.Combine(itemPhotosRoot, "avif", "1440p"));
        paths.Add(Path.Combine(itemPhotosRoot, "avif", "4k"));

        paths.Add(Path.Combine(itemThumbsRoot, "jxl", "hd"));
        paths.Add(Path.Combine(itemThumbsRoot, "jxl", "1440p"));
        paths.Add(Path.Combine(itemThumbsRoot, "jxl", "4k"));
        paths.Add(Path.Combine(itemPhotosRoot, "jxl", "hd"));
        paths.Add(Path.Combine(itemPhotosRoot, "jxl", "1440p"));
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

            // Check if JXL hd thumbnail already exists — if so, assume all variants exist
            string checkPath = Path.Combine(itemPhotosRoot, "thumbs", "jxl", "hd", $"{id}.jxl");

            if(File.Exists(checkPath)) continue;

            Console.WriteLine("Backfilling JXL for {0}...", id);

            foreach(string resolution in new[] { "hd", "1440p", "4k" })
            {
                Convert(assetRootPath, id, originalFile, sourceFormat, "JXL", resolution, true,  scan, item);
                Convert(assetRootPath, id, originalFile, sourceFormat, "JXL", resolution, false, scan, item);
            }
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
            case "hd":
                if(thumbnail)
                {
                    width  = 256;
                    height = 256;
                }
                else
                {
                    width  = 1920;
                    height = 1080;
                }

                break;
            case "1440p":
                if(thumbnail)
                {
                    width  = 384;
                    height = 384;
                }
                else
                {
                    width  = 2560;
                    height = 1440;
                }

                break;
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

        string tmpPath;
        bool   ret;

        switch(outputFormat)
        {
            case "jpeg":
                outputPath = Path.Combine(outputPath, $"{id}.jpg");

                return ConvertUsingImageMagick(originalPath, outputPath, width, height);
            case "webp":
                outputPath = Path.Combine(outputPath, $"{id}.webp");

                return ConvertUsingImageMagick(originalPath, outputPath, width, height);

            case "avif":
                outputPath = Path.Combine(outputPath, $"{id}.avif");

                tmpPath = Path.GetTempFileName();
                File.Delete(tmpPath);
                tmpPath += ".png";

                // AVIFENC does not resize
                ret = ConvertUsingImageMagick(originalPath, tmpPath, width, height);

                if(!ret)
                {
                    File.Delete(tmpPath);

                    return ret;
                }

                ret = ConvertToAvif(tmpPath, outputPath, width, height);

                File.Delete(tmpPath);

                return ret;

            case "jxl":
                outputPath = Path.Combine(outputPath, $"{id}.jxl");

                return ConvertToJxl(originalPath, outputPath, width, height);
            default:
                return false;
        }
    }

    public static bool ConvertToJxl(string originalPath, string outputPath, int width, int height)
    {
        var convert = new Process
        {
            StartInfo =
            {
                FileName               = "convert",
                CreateNoWindow         = true,
                RedirectStandardError  = true,
                RedirectStandardOutput = true,
                ArgumentList =
                {
                    "-resize",
                    $"{width}x{height}>",
                    "-strip",
                    "-define",
                    "jxl:encoder=vardct",
                    "-define",
                    "jxl:distance=4",
                    "-define",
                    "jxl:effort=9",
                    originalPath,
                    outputPath
                }
            }
        };

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

    public static bool ConvertUsingImageMagick(string originalPath, string outputPath, int width, int height)
    {
        var convert = new Process
        {
            StartInfo =
            {
                FileName               = "convert",
                CreateNoWindow         = true,
                RedirectStandardError  = true,
                RedirectStandardOutput = true,
                ArgumentList =
                {
                    "-resize",
                    $"{width}x{height}>",
                    "-strip",
                    originalPath,
                    outputPath
                }
            }
        };

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

    public static bool ConvertToAvif(string originalPath, string outputPath, int width, int height)
    {
        var avif = new Process
        {
            StartInfo =
            {
                FileName               = "avifenc",
                CreateNoWindow         = true,
                RedirectStandardError  = true,
                RedirectStandardOutput = true,
                ArgumentList =
                {
                    "-j",
                    "4",
                    originalPath,
                    outputPath
                }
            }
        };

        try
        {
            avif.Start();
            avif.StandardOutput.ReadToEnd();
            avif.WaitForExit();

            return avif.ExitCode == 0;
        }
        catch(Exception)
        {
            return false;
        }
    }

    public void ConversionWorker(string assetRootPath, Guid id, string originalFilePath, string sourceFormat, bool scan,
                                 string item)
    {
        List<Task> pool =
        [
            new(() => { bool r = Convert(assetRootPath, id, originalFilePath, sourceFormat, "JPEG", "4k",    true,  scan, item); FinishedRenderingJpeg4kThumbnail?.Invoke(r); }),
            new(() => { bool r = Convert(assetRootPath, id, originalFilePath, sourceFormat, "JPEG", "1440p", true,  scan, item); FinishedRenderingJpeg1440Thumbnail?.Invoke(r); }),
            new(() => { bool r = Convert(assetRootPath, id, originalFilePath, sourceFormat, "JPEG", "hd",    true,  scan, item); FinishedRenderingJpegHdThumbnail?.Invoke(r); }),
            new(() => { bool r = Convert(assetRootPath, id, originalFilePath, sourceFormat, "JPEG", "4k",    false, scan, item); FinishedRenderingJpeg4K?.Invoke(r); }),
            new(() => { bool r = Convert(assetRootPath, id, originalFilePath, sourceFormat, "JPEG", "1440p", false, scan, item); FinishedRenderingJpeg1440?.Invoke(r); }),
            new(() => { bool r = Convert(assetRootPath, id, originalFilePath, sourceFormat, "JPEG", "hd",    false, scan, item); FinishedRenderingJpegHd?.Invoke(r); }),
            new(() => { bool r = Convert(assetRootPath, id, originalFilePath, sourceFormat, "WEBP", "4k",    true,  scan, item); FinishedRenderingWebp4kThumbnail?.Invoke(r); }),
            new(() => { bool r = Convert(assetRootPath, id, originalFilePath, sourceFormat, "WEBP", "1440p", true,  scan, item); FinishedRenderingWebp1440Thumbnail?.Invoke(r); }),
            new(() => { bool r = Convert(assetRootPath, id, originalFilePath, sourceFormat, "WEBP", "hd",    true,  scan, item); FinishedRenderingWebpHdThumbnail?.Invoke(r); }),
            new(() => { bool r = Convert(assetRootPath, id, originalFilePath, sourceFormat, "WEBP", "4k",    false, scan, item); FinishedRenderingWebp4k?.Invoke(r); }),
            new(() => { bool r = Convert(assetRootPath, id, originalFilePath, sourceFormat, "WEBP", "1440p", false, scan, item); FinishedRenderingWebp1440?.Invoke(r); }),
            new(() => { bool r = Convert(assetRootPath, id, originalFilePath, sourceFormat, "WEBP", "hd",    false, scan, item); FinishedRenderingWebpHd?.Invoke(r); }),
            new(() => { bool r = Convert(assetRootPath, id, originalFilePath, sourceFormat, "AVIF", "4k",    true,  scan, item); FinishedRenderingAvif4kThumbnail?.Invoke(r); }),
            new(() => { bool r = Convert(assetRootPath, id, originalFilePath, sourceFormat, "AVIF", "1440p", true,  scan, item); FinishedRenderingAvif1440Thumbnail?.Invoke(r); }),
            new(() => { bool r = Convert(assetRootPath, id, originalFilePath, sourceFormat, "AVIF", "hd",    true,  scan, item); FinishedRenderingAvifHdThumbnail?.Invoke(r); }),
            new(() => { bool r = Convert(assetRootPath, id, originalFilePath, sourceFormat, "AVIF", "4k",    false, scan, item); FinishedRenderingAvif4K?.Invoke(r); }),
            new(() => { bool r = Convert(assetRootPath, id, originalFilePath, sourceFormat, "AVIF", "1440p", false, scan, item); FinishedRenderingAvif1440?.Invoke(r); }),
            new(() => { bool r = Convert(assetRootPath, id, originalFilePath, sourceFormat, "AVIF", "hd",    false, scan, item); FinishedRenderingAvifHd?.Invoke(r); }),
            new(() => { bool r = Convert(assetRootPath, id, originalFilePath, sourceFormat, "JXL",  "4k",    true,  scan, item); FinishedRenderingJxl4kThumbnail?.Invoke(r); }),
            new(() => { bool r = Convert(assetRootPath, id, originalFilePath, sourceFormat, "JXL",  "1440p", true,  scan, item); FinishedRenderingJxl1440Thumbnail?.Invoke(r); }),
            new(() => { bool r = Convert(assetRootPath, id, originalFilePath, sourceFormat, "JXL",  "hd",    true,  scan, item); FinishedRenderingJxlHdThumbnail?.Invoke(r); }),
            new(() => { bool r = Convert(assetRootPath, id, originalFilePath, sourceFormat, "JXL",  "4k",    false, scan, item); FinishedRenderingJxl4K?.Invoke(r); }),
            new(() => { bool r = Convert(assetRootPath, id, originalFilePath, sourceFormat, "JXL",  "1440p", false, scan, item); FinishedRenderingJxl1440?.Invoke(r); }),
            new(() => { bool r = Convert(assetRootPath, id, originalFilePath, sourceFormat, "JXL",  "hd",    false, scan, item); FinishedRenderingJxlHd?.Invoke(r); })
        ];

        foreach(Task thread in pool) thread.Start();

        Task.WaitAll(pool.ToArray());

        FinishedAll?.Invoke(true);
    }

    public event ConversionFinished FinishedAll;

    public event ConversionFinished FinishedRenderingJpegHdThumbnail;
    public event ConversionFinished FinishedRenderingJpeg1440Thumbnail;
    public event ConversionFinished FinishedRenderingJpeg4kThumbnail;
    public event ConversionFinished FinishedRenderingJpegHd;
    public event ConversionFinished FinishedRenderingJpeg1440;
    public event ConversionFinished FinishedRenderingJpeg4K;
    public event ConversionFinished FinishedRenderingWebpHdThumbnail;
    public event ConversionFinished FinishedRenderingWebp1440Thumbnail;
    public event ConversionFinished FinishedRenderingWebp4kThumbnail;
    public event ConversionFinished FinishedRenderingWebpHd;
    public event ConversionFinished FinishedRenderingWebp1440;
    public event ConversionFinished FinishedRenderingWebp4k;
    public event ConversionFinished FinishedRenderingAvifHdThumbnail;
    public event ConversionFinished FinishedRenderingAvif1440Thumbnail;
    public event ConversionFinished FinishedRenderingAvif4kThumbnail;
    public event ConversionFinished FinishedRenderingAvifHd;
    public event ConversionFinished FinishedRenderingAvif1440;
    public event ConversionFinished FinishedRenderingAvif4K;
    public event ConversionFinished FinishedRenderingJxlHdThumbnail;
    public event ConversionFinished FinishedRenderingJxl1440Thumbnail;
    public event ConversionFinished FinishedRenderingJxl4kThumbnail;
    public event ConversionFinished FinishedRenderingJxlHd;
    public event ConversionFinished FinishedRenderingJxl1440;
    public event ConversionFinished FinishedRenderingJxl4K;
}