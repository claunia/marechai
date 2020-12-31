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
// Copyright © 2003-2021 Natalia Portillo
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Marechai.Helpers
{
    public class Photos
    {
        public delegate Task ConversionFinished(bool result);

        public static void EnsureCreated(string webRootPath, bool scan, string item)
        {
            List<string> paths = new();

            string photosRoot             = Path.Combine(webRootPath, "assets", scan ? "scan" : "photos");
            string itemPhotosRoot         = Path.Combine(photosRoot, item);
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

            paths.Add(Path.Combine(itemThumbsRoot, "jp2k", "hd"));
            paths.Add(Path.Combine(itemThumbsRoot, "jp2k", "1440p"));
            paths.Add(Path.Combine(itemThumbsRoot, "jp2k", "4k"));
            paths.Add(Path.Combine(itemPhotosRoot, "jp2k", "hd"));
            paths.Add(Path.Combine(itemPhotosRoot, "jp2k", "1440p"));
            paths.Add(Path.Combine(itemPhotosRoot, "jp2k", "4k"));

            paths.Add(Path.Combine(itemThumbsRoot, "webp", "hd"));
            paths.Add(Path.Combine(itemThumbsRoot, "webp", "1440p"));
            paths.Add(Path.Combine(itemThumbsRoot, "webp", "4k"));
            paths.Add(Path.Combine(itemPhotosRoot, "webp", "hd"));
            paths.Add(Path.Combine(itemPhotosRoot, "webp", "1440p"));
            paths.Add(Path.Combine(itemPhotosRoot, "webp", "4k"));

            paths.Add(Path.Combine(itemThumbsRoot, "heif", "hd"));
            paths.Add(Path.Combine(itemThumbsRoot, "heif", "1440p"));
            paths.Add(Path.Combine(itemThumbsRoot, "heif", "4k"));
            paths.Add(Path.Combine(itemPhotosRoot, "heif", "hd"));
            paths.Add(Path.Combine(itemPhotosRoot, "heif", "1440p"));
            paths.Add(Path.Combine(itemPhotosRoot, "heif", "4k"));

            paths.Add(Path.Combine(itemThumbsRoot, "avif", "hd"));
            paths.Add(Path.Combine(itemThumbsRoot, "avif", "1440p"));
            paths.Add(Path.Combine(itemThumbsRoot, "avif", "4k"));
            paths.Add(Path.Combine(itemPhotosRoot, "avif", "hd"));
            paths.Add(Path.Combine(itemPhotosRoot, "avif", "1440p"));
            paths.Add(Path.Combine(itemPhotosRoot, "avif", "4k"));

            foreach(string path in paths.Where(path => !Directory.Exists(path)))
                Directory.CreateDirectory(path);
        }

        public static bool Convert(string webRootPath, Guid id, string originalPath, string sourceFormat,
                                   string outputFormat, string resolution, bool thumbnail, bool scan, string item)
        {
            outputFormat = outputFormat.ToLowerInvariant();
            resolution   = resolution.ToLowerInvariant();
            sourceFormat = sourceFormat.ToLowerInvariant();

            string outputPath = Path.Combine(webRootPath, "assets", scan ? "scans" : "photos", item);
            int    width, height;

            if(thumbnail)
                outputPath = Path.Combine(outputPath, "thumbs");

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
                default: return false;
            }

            string tmpPath;
            bool   ret;

            switch(outputFormat)
            {
                case "jpeg":
                    outputPath = Path.Combine(outputPath, $"{id}.jpg");

                    return ConvertUsingImageMagick(originalPath, outputPath, width, height);
                case "jp2k":
                    outputPath = Path.Combine(outputPath, $"{id}.jp2");

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
                default: return false;
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
                        $"{width}x{height}",
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

        public void ConversionWorker(string webRootPath, Guid id, string originalFilePath, string sourceFormat,
                                     bool scan, string item)
        {
            List<Task> pool = new()
            {
                new(() => FinishedRenderingJpeg4kThumbnail?.Invoke(Convert(webRootPath, id, originalFilePath,
                                                                           sourceFormat, "JPEG", "4k", true, scan,
                                                                           item))),
                new(() => FinishedRenderingJpeg1440Thumbnail?.Invoke(Convert(webRootPath, id, originalFilePath,
                                                                             sourceFormat, "JPEG", "1440p", true, scan,
                                                                             item))),
                new(() => FinishedRenderingJpegHdThumbnail?.Invoke(Convert(webRootPath, id, originalFilePath,
                                                                           sourceFormat, "JPEG", "hd", true, scan,
                                                                           item))),
                new(() => FinishedRenderingJpeg4k?.Invoke(Convert(webRootPath, id, originalFilePath, sourceFormat,
                                                                  "JPEG", "4k", false, scan, item))),
                new(() => FinishedRenderingJpeg1440?.Invoke(Convert(webRootPath, id, originalFilePath, sourceFormat,
                                                                    "JPEG", "1440p", false, scan, item))),
                new(() => FinishedRenderingJpegHd?.Invoke(Convert(webRootPath, id, originalFilePath, sourceFormat,
                                                                  "JPEG", "hd", false, scan, item))),
                new(() => FinishedRenderingJp2k4kThumbnail?.Invoke(Convert(webRootPath, id, originalFilePath,
                                                                           sourceFormat, "JP2K", "4k", true, scan,
                                                                           item))),
                new(() => FinishedRenderingJp2k1440Thumbnail?.Invoke(Convert(webRootPath, id, originalFilePath,
                                                                             sourceFormat, "JP2K", "1440p", true, scan,
                                                                             item))),
                new(() => FinishedRenderingJp2kHdThumbnail?.Invoke(Convert(webRootPath, id, originalFilePath,
                                                                           sourceFormat, "JP2K", "hd", true, scan,
                                                                           item))),
                new(() => FinishedRenderingJp2k4k?.Invoke(Convert(webRootPath, id, originalFilePath, sourceFormat,
                                                                  "JP2K", "4k", false, scan, item))),
                new(() => FinishedRenderingJp2k1440?.Invoke(Convert(webRootPath, id, originalFilePath, sourceFormat,
                                                                    "JP2K", "1440p", false, scan, item))),
                new(() => FinishedRenderingJp2kHd?.Invoke(Convert(webRootPath, id, originalFilePath, sourceFormat,
                                                                  "JP2K", "hd", false, scan, item))),
                new(() => FinishedRenderingWebp4kThumbnail?.Invoke(Convert(webRootPath, id, originalFilePath,
                                                                           sourceFormat, "WEBP", "4k", true, scan,
                                                                           item))),
                new(() => FinishedRenderingWebp1440Thumbnail?.Invoke(Convert(webRootPath, id, originalFilePath,
                                                                             sourceFormat, "WEBP", "1440p", true, scan,
                                                                             item))),
                new(() => FinishedRenderingWebpHdThumbnail?.Invoke(Convert(webRootPath, id, originalFilePath,
                                                                           sourceFormat, "WEBP", "hd", true, scan,
                                                                           item))),
                new(() => FinishedRenderingWebp4k?.Invoke(Convert(webRootPath, id, originalFilePath, sourceFormat,
                                                                  "WEBP", "4k", false, scan, item))),
                new(() => FinishedRenderingWebp1440?.Invoke(Convert(webRootPath, id, originalFilePath, sourceFormat,
                                                                    "WEBP", "1440p", false, scan, item))),
                new(() => FinishedRenderingWebpHd?.Invoke(Convert(webRootPath, id, originalFilePath, sourceFormat,
                                                                  "WEBP", "hd", false, scan, item))),
                new(() => FinishedRenderingHeif4kThumbnail?.Invoke(Convert(webRootPath, id, originalFilePath,
                                                                           sourceFormat, "HEIF", "4k", true, scan,
                                                                           item))),
                new(() => FinishedRenderingHeif1440Thumbnail?.Invoke(Convert(webRootPath, id, originalFilePath,
                                                                             sourceFormat, "HEIF", "1440p", true, scan,
                                                                             item))),
                new(() => FinishedRenderingHeifHdThumbnail?.Invoke(Convert(webRootPath, id, originalFilePath,
                                                                           sourceFormat, "HEIF", "hd", true, scan,
                                                                           item))),
                new(() => FinishedRenderingHeif4k?.Invoke(Convert(webRootPath, id, originalFilePath, sourceFormat,
                                                                  "HEIF", "4k", false, scan, item))),
                new(() => FinishedRenderingHeif1440?.Invoke(Convert(webRootPath, id, originalFilePath, sourceFormat,
                                                                    "HEIF", "1440p", false, scan, item))),
                new(() => FinishedRenderingHeifHd?.Invoke(Convert(webRootPath, id, originalFilePath, sourceFormat,
                                                                  "HEIF", "hd", false, scan, item))),
                new(() => FinishedRenderingAvif4kThumbnail?.Invoke(Convert(webRootPath, id, originalFilePath,
                                                                           sourceFormat, "AVIF", "4k", true, scan,
                                                                           item))),
                new(() => FinishedRenderingAvif1440Thumbnail?.Invoke(Convert(webRootPath, id, originalFilePath,
                                                                             sourceFormat, "AVIF", "1440p", true, scan,
                                                                             item))),
                new(() => FinishedRenderingAvifHdThumbnail?.Invoke(Convert(webRootPath, id, originalFilePath,
                                                                           sourceFormat, "AVIF", "hd", true, scan,
                                                                           item))),
                new(() => FinishedRenderingAvif4k?.Invoke(Convert(webRootPath, id, originalFilePath, sourceFormat,
                                                                  "AVIF", "4k", false, scan, item))),
                new(() => FinishedRenderingAvif1440?.Invoke(Convert(webRootPath, id, originalFilePath, sourceFormat,
                                                                    "AVIF", "1440p", false, scan, item))),
                new(() => FinishedRenderingAvifHd?.Invoke(Convert(webRootPath, id, originalFilePath, sourceFormat,
                                                                  "AVIF", "hd", false, scan, item)))
            };

            foreach(Task thread in pool)
                thread.Start();

            Task.WaitAll(pool.ToArray());

            FinishedAll?.Invoke(true);
        }

        public event ConversionFinished FinishedAll;

        public event ConversionFinished FinishedRenderingJpegHdThumbnail;
        public event ConversionFinished FinishedRenderingJpeg1440Thumbnail;
        public event ConversionFinished FinishedRenderingJpeg4kThumbnail;
        public event ConversionFinished FinishedRenderingJpegHd;
        public event ConversionFinished FinishedRenderingJpeg1440;
        public event ConversionFinished FinishedRenderingJpeg4k;
        public event ConversionFinished FinishedRenderingJp2kHdThumbnail;
        public event ConversionFinished FinishedRenderingJp2k1440Thumbnail;
        public event ConversionFinished FinishedRenderingJp2k4kThumbnail;
        public event ConversionFinished FinishedRenderingJp2kHd;
        public event ConversionFinished FinishedRenderingJp2k1440;
        public event ConversionFinished FinishedRenderingJp2k4k;
        public event ConversionFinished FinishedRenderingWebpHdThumbnail;
        public event ConversionFinished FinishedRenderingWebp1440Thumbnail;
        public event ConversionFinished FinishedRenderingWebp4kThumbnail;
        public event ConversionFinished FinishedRenderingWebpHd;
        public event ConversionFinished FinishedRenderingWebp1440;
        public event ConversionFinished FinishedRenderingWebp4k;
        public event ConversionFinished FinishedRenderingHeifHdThumbnail;
        public event ConversionFinished FinishedRenderingHeif1440Thumbnail;
        public event ConversionFinished FinishedRenderingHeif4kThumbnail;
        public event ConversionFinished FinishedRenderingHeifHd;
        public event ConversionFinished FinishedRenderingHeif1440;
        public event ConversionFinished FinishedRenderingHeif4k;
        public event ConversionFinished FinishedRenderingAvifHdThumbnail;
        public event ConversionFinished FinishedRenderingAvif1440Thumbnail;
        public event ConversionFinished FinishedRenderingAvif4kThumbnail;
        public event ConversionFinished FinishedRenderingAvifHd;
        public event ConversionFinished FinishedRenderingAvif1440;
        public event ConversionFinished FinishedRenderingAvif4k;
    }
}