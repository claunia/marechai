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

        public static void EnsureCreated(string webRootPath)
        {
            List<string> paths = new List<string>();

            string photosRoot                = Path.Combine(webRootPath, "assets", "photos");
            string machinePhotosRoot         = Path.Combine(photosRoot, "machines");
            string machineThumbsRoot         = Path.Combine(machinePhotosRoot, "thumbs");
            string machineOriginalPhotosRoot = Path.Combine(machinePhotosRoot, "originals");

            paths.Add(photosRoot);
            paths.Add(machinePhotosRoot);
            paths.Add(machineThumbsRoot);
            paths.Add(machineOriginalPhotosRoot);

            paths.Add(Path.Combine(machineThumbsRoot, "jpeg", "hd"));
            paths.Add(Path.Combine(machineThumbsRoot, "jpeg", "1440p"));
            paths.Add(Path.Combine(machineThumbsRoot, "jpeg", "4k"));
            paths.Add(Path.Combine(machinePhotosRoot, "jpeg", "hd"));
            paths.Add(Path.Combine(machinePhotosRoot, "jpeg", "1440p"));
            paths.Add(Path.Combine(machinePhotosRoot, "jpeg", "4k"));

            paths.Add(Path.Combine(machineThumbsRoot, "jp2k", "hd"));
            paths.Add(Path.Combine(machineThumbsRoot, "jp2k", "1440p"));
            paths.Add(Path.Combine(machineThumbsRoot, "jp2k", "4k"));
            paths.Add(Path.Combine(machinePhotosRoot, "jp2k", "hd"));
            paths.Add(Path.Combine(machinePhotosRoot, "jp2k", "1440p"));
            paths.Add(Path.Combine(machinePhotosRoot, "jp2k", "4k"));

            paths.Add(Path.Combine(machineThumbsRoot, "webp", "hd"));
            paths.Add(Path.Combine(machineThumbsRoot, "webp", "1440p"));
            paths.Add(Path.Combine(machineThumbsRoot, "webp", "4k"));
            paths.Add(Path.Combine(machinePhotosRoot, "webp", "hd"));
            paths.Add(Path.Combine(machinePhotosRoot, "webp", "1440p"));
            paths.Add(Path.Combine(machinePhotosRoot, "webp", "4k"));

            paths.Add(Path.Combine(machineThumbsRoot, "heif", "hd"));
            paths.Add(Path.Combine(machineThumbsRoot, "heif", "1440p"));
            paths.Add(Path.Combine(machineThumbsRoot, "heif", "4k"));
            paths.Add(Path.Combine(machinePhotosRoot, "heif", "hd"));
            paths.Add(Path.Combine(machinePhotosRoot, "heif", "1440p"));
            paths.Add(Path.Combine(machinePhotosRoot, "heif", "4k"));

            paths.Add(Path.Combine(machineThumbsRoot, "avif", "hd"));
            paths.Add(Path.Combine(machineThumbsRoot, "avif", "1440p"));
            paths.Add(Path.Combine(machineThumbsRoot, "avif", "4k"));
            paths.Add(Path.Combine(machinePhotosRoot, "avif", "hd"));
            paths.Add(Path.Combine(machinePhotosRoot, "avif", "1440p"));
            paths.Add(Path.Combine(machinePhotosRoot, "avif", "4k"));

            foreach(string path in paths.Where(path => !Directory.Exists(path)))
                Directory.CreateDirectory(path);
        }

        public static bool Convert(string webRootPath, Guid id, string originalPath, string sourceFormat,
                                   string outputFormat, string resolution, bool thumbnail)
        {
            outputFormat = outputFormat.ToLowerInvariant();
            resolution   = resolution.ToLowerInvariant();
            sourceFormat = sourceFormat.ToLowerInvariant();

            string outputPath = Path.Combine(webRootPath, "assets", "photos", "machines");
            int    width, height;

            if(thumbnail)
                outputPath = Path.Combine(outputPath, "thumbs");

            outputPath = Path.Combine(outputPath, outputFormat);
            outputPath = Path.Combine(outputPath, resolution);

            switch(resolution)
            {
                case"hd":
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
                case"1440p":
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
                case"4k":
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
                case"jpeg":
                    outputPath = Path.Combine(outputPath, $"{id}.jpg");

                    return ConvertUsingImageMagick(originalPath, outputPath, width, height);
                case"jp2k":
                    outputPath = Path.Combine(outputPath, $"{id}.jp2");

                    return ConvertUsingImageMagick(originalPath, outputPath, width, height);

                case"webp":
                    outputPath = Path.Combine(outputPath, $"{id}.webp");

                    return ConvertUsingImageMagick(originalPath, outputPath, width, height);

                case"heif":
                    outputPath = Path.Combine(outputPath, $"{id}.heic");

                    return ConvertUsingImageMagick(originalPath, outputPath, width, height);

                case"avif":
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
                    FileName               = "convert", CreateNoWindow = true, RedirectStandardError = true,
                    RedirectStandardOutput = true, ArgumentList =
                    {
                        "-resize", $"{width}x{height}", "-strip", originalPath,
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
                    FileName               = "avifenc", CreateNoWindow = true, RedirectStandardError = true,
                    RedirectStandardOutput = true, ArgumentList =
                    {
                        "-j", "4", originalPath, outputPath
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

        public void ConversionWorker(string webRootPath, Guid id, string originalFilePath, string sourceFormat)
        {
            List<Task> pool = new List<Task>
            {
                new Task(() => FinishedRenderingJpeg4kThumbnail?.Invoke(Convert(webRootPath, id, originalFilePath,
                                                                                sourceFormat, "JPEG", "4k", true))),
                new Task(() => FinishedRenderingJpeg1440Thumbnail?.Invoke(Convert(webRootPath, id, originalFilePath,
                                                                                  sourceFormat, "JPEG", "1440p",
                                                                                  true))),
                new Task(() => FinishedRenderingJpegHdThumbnail?.Invoke(Convert(webRootPath, id, originalFilePath,
                                                                                sourceFormat, "JPEG", "hd", true))),
                new Task(() => FinishedRenderingJpeg4k?.Invoke(Convert(webRootPath, id, originalFilePath, sourceFormat,
                                                                       "JPEG", "4k", false))),
                new Task(() => FinishedRenderingJpeg1440?.Invoke(Convert(webRootPath, id, originalFilePath,
                                                                         sourceFormat, "JPEG", "1440p", false))),
                new Task(() => FinishedRenderingJpegHd?.Invoke(Convert(webRootPath, id, originalFilePath, sourceFormat,
                                                                       "JPEG", "hd", false))),
                new Task(() => FinishedRenderingJp2k4kThumbnail?.Invoke(Convert(webRootPath, id, originalFilePath,
                                                                                sourceFormat, "JP2K", "4k", true))),
                new Task(() => FinishedRenderingJp2k1440Thumbnail?.Invoke(Convert(webRootPath, id, originalFilePath,
                                                                                  sourceFormat, "JP2K", "1440p",
                                                                                  true))),
                new Task(() => FinishedRenderingJp2kHdThumbnail?.Invoke(Convert(webRootPath, id, originalFilePath,
                                                                                sourceFormat, "JP2K", "hd", true))),
                new Task(() => FinishedRenderingJp2k4k?.Invoke(Convert(webRootPath, id, originalFilePath, sourceFormat,
                                                                       "JP2K", "4k", false))),
                new Task(() => FinishedRenderingJp2k1440?.Invoke(Convert(webRootPath, id, originalFilePath,
                                                                         sourceFormat, "JP2K", "1440p", false))),
                new Task(() => FinishedRenderingJp2kHd?.Invoke(Convert(webRootPath, id, originalFilePath, sourceFormat,
                                                                       "JP2K", "hd", false))),
                new Task(() => FinishedRenderingWebp4kThumbnail?.Invoke(Convert(webRootPath, id, originalFilePath,
                                                                                sourceFormat, "WEBP", "4k", true))),
                new Task(() => FinishedRenderingWebp1440Thumbnail?.Invoke(Convert(webRootPath, id, originalFilePath,
                                                                                  sourceFormat, "WEBP", "1440p",
                                                                                  true))),
                new Task(() => FinishedRenderingWebpHdThumbnail?.Invoke(Convert(webRootPath, id, originalFilePath,
                                                                                sourceFormat, "WEBP", "hd", true))),
                new Task(() => FinishedRenderingWebp4k?.Invoke(Convert(webRootPath, id, originalFilePath, sourceFormat,
                                                                       "WEBP", "4k", false))),
                new Task(() => FinishedRenderingWebp1440?.Invoke(Convert(webRootPath, id, originalFilePath,
                                                                         sourceFormat, "WEBP", "1440p", false))),
                new Task(() => FinishedRenderingWebpHd?.Invoke(Convert(webRootPath, id, originalFilePath, sourceFormat,
                                                                       "WEBP", "hd", false))),
                new Task(() => FinishedRenderingHeif4kThumbnail?.Invoke(Convert(webRootPath, id, originalFilePath,
                                                                                sourceFormat, "HEIF", "4k", true))),
                new Task(() => FinishedRenderingHeif1440Thumbnail?.Invoke(Convert(webRootPath, id, originalFilePath,
                                                                                  sourceFormat, "HEIF", "1440p",
                                                                                  true))),
                new Task(() => FinishedRenderingHeifHdThumbnail?.Invoke(Convert(webRootPath, id, originalFilePath,
                                                                                sourceFormat, "HEIF", "hd", true))),
                new Task(() => FinishedRenderingHeif4k?.Invoke(Convert(webRootPath, id, originalFilePath, sourceFormat,
                                                                       "HEIF", "4k", false))),
                new Task(() => FinishedRenderingHeif1440?.Invoke(Convert(webRootPath, id, originalFilePath,
                                                                         sourceFormat, "HEIF", "1440p", false))),
                new Task(() => FinishedRenderingHeifHd?.Invoke(Convert(webRootPath, id, originalFilePath, sourceFormat,
                                                                       "HEIF", "hd", false))),
                new Task(() => FinishedRenderingAvif4kThumbnail?.Invoke(Convert(webRootPath, id, originalFilePath,
                                                                                sourceFormat, "AVIF", "4k", true))),
                new Task(() => FinishedRenderingAvif1440Thumbnail?.Invoke(Convert(webRootPath, id, originalFilePath,
                                                                                  sourceFormat, "AVIF", "1440p",
                                                                                  true))),
                new Task(() => FinishedRenderingAvifHdThumbnail?.Invoke(Convert(webRootPath, id, originalFilePath,
                                                                                sourceFormat, "AVIF", "hd", true))),
                new Task(() => FinishedRenderingAvif4k?.Invoke(Convert(webRootPath, id, originalFilePath, sourceFormat,
                                                                       "AVIF", "4k", false))),
                new Task(() => FinishedRenderingAvif1440?.Invoke(Convert(webRootPath, id, originalFilePath,
                                                                         sourceFormat, "AVIF", "1440p", false))),
                new Task(() => FinishedRenderingAvifHd?.Invoke(Convert(webRootPath, id, originalFilePath, sourceFormat,
                                                                       "AVIF", "hd", false)))
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