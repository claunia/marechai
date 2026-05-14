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
        string[] formats     = ["JPEG", "WEBP", "AVIF", "JXL"];
        string[] resolutions = ["4k"];

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

}
