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

        foreach(string format in new[] { "jpeg", "webp", "heif", "avif", "jxl" })
        {
            foreach(string resolution in new[] { "hd", "1440p", "4k" })
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
        string[] formats     = ["JPEG", "WEBP", "HEIF", "AVIF", "JXL"];
        string[] resolutions = ["hd", "1440p", "4k"];

        List<Task> pool = [];

        foreach(string format in formats)
        {
            foreach(string resolution in resolutions)
            {
                string f = format;
                string r = resolution;

                pool.Add(new Task(() =>
                {
                    bool thumbResult = Convert(assetRootPath, id, originalFilePath, sourceFormat, f, r, true, itemName);
                    bool fullResult  = Convert(assetRootPath, id, originalFilePath, sourceFormat, f, r, false, itemName);

                    if(!thumbResult)
                        Console.WriteLine($"\e[33m    Warning: {f} {r} thumbnail conversion failed\e[0m");

                    if(!fullResult)
                        Console.WriteLine($"\e[33m    Warning: {f} {r} full conversion failed\e[0m");
                }));
            }
        }

        foreach(Task thread in pool) thread.Start();

        Task.WaitAll(pool.ToArray());
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
            case "hd":
                if(thumbnail) { width = 256;  height = 256; }
                else          { width = 1920; height = 1080; }

                break;
            case "1440p":
                if(thumbnail) { width = 384;  height = 384; }
                else          { width = 2560; height = 1440; }

                break;
            case "4k":
                if(thumbnail) { width = 512;  height = 512; }
                else          { width = 3840; height = 2160; }

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

            case "heif":
                outputPath = Path.Combine(outputPath, $"{id}.heic");

                return ConvertUsingImageMagick(originalPath, outputPath, width, height);

            case "avif":
                outputPath = Path.Combine(outputPath, $"{id}.avif");

                tmpPath = Path.GetTempFileName();
                File.Delete(tmpPath);
                tmpPath += ".png";

                ret = ConvertUsingImageMagick(originalPath, tmpPath, width, height);

                if(!ret)
                {
                    File.Delete(tmpPath);

                    return ret;
                }

                ret = ConvertToAvif(tmpPath, outputPath);

                File.Delete(tmpPath);

                return ret;

            case "jxl":
                outputPath = Path.Combine(outputPath, $"{id}.jxl");

                return ConvertToJxl(originalPath, outputPath, width, height);

            default:
                return false;
        }
    }

    static bool ConvertToJxl(string originalPath, string outputPath, int width, int height)
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

    static bool ConvertUsingImageMagick(string originalPath, string outputPath, int width, int height)
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

    static bool ConvertToAvif(string originalPath, string outputPath)
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

}
