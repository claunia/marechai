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

                return ConvertUsingImageMagick(originalPath, outputPath, width, height);

            case "jxl":
                outputPath = Path.Combine(outputPath, $"{id}.jxl");

                return ConvertUsingImageMagick(originalPath, outputPath, width, height);
            default:
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
                    "-quality",
                    "80",
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

    public void ConversionWorker(string assetRootPath, Guid id, string originalFilePath, string sourceFormat, bool scan,
                                 string item)
    {
        List<Task> pool =
        [
            new(() => { bool r = Convert(assetRootPath, id, originalFilePath, sourceFormat, "JPEG", "4k", true,  scan, item); FinishedRenderingJpeg4kThumbnail?.Invoke(r); }),
            new(() => { bool r = Convert(assetRootPath, id, originalFilePath, sourceFormat, "JPEG", "4k", false, scan, item); FinishedRenderingJpeg4K?.Invoke(r); }),
            new(() => { bool r = Convert(assetRootPath, id, originalFilePath, sourceFormat, "WEBP", "4k", true,  scan, item); FinishedRenderingWebp4kThumbnail?.Invoke(r); }),
            new(() => { bool r = Convert(assetRootPath, id, originalFilePath, sourceFormat, "WEBP", "4k", false, scan, item); FinishedRenderingWebp4k?.Invoke(r); }),
            new(() => { bool r = Convert(assetRootPath, id, originalFilePath, sourceFormat, "AVIF", "4k", true,  scan, item); FinishedRenderingAvif4kThumbnail?.Invoke(r); }),
            new(() => { bool r = Convert(assetRootPath, id, originalFilePath, sourceFormat, "AVIF", "4k", false, scan, item); FinishedRenderingAvif4K?.Invoke(r); }),
            new(() => { bool r = Convert(assetRootPath, id, originalFilePath, sourceFormat, "JXL",  "4k", true,  scan, item); FinishedRenderingJxl4kThumbnail?.Invoke(r); }),
            new(() => { bool r = Convert(assetRootPath, id, originalFilePath, sourceFormat, "JXL",  "4k", false, scan, item); FinishedRenderingJxl4K?.Invoke(r); })
        ];

        foreach(Task thread in pool) thread.Start();

        Task.WaitAll(pool.ToArray());

        FinishedAll?.Invoke(true);
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
}