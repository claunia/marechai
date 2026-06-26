using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Marechai.Igdb.Services;

/// <summary>
///     Downloads IGDB CDN images, trying several size candidates and keeping whichever decodes
///     to the largest actual pixel area. IGDB's documented size identifiers
///     (https://api-docs.igdb.com/#images) top out at <c>1080p</c>, with no documented
///     <c>original</c> size — but <c>t_original</c> works in practice for some image types, and
///     any size accepts a <c>_2x</c> retina suffix that Cloudinary won't upscale past the source's
///     native resolution. None of this is fully documented, so candidates are never trusted blindly:
///     each is downloaded and measured, and the actually-largest one wins.
/// </summary>
internal static class IgdbImageDownloader
{
    public static readonly string[] LogoSizeCandidates = { "logo_med_2x", "logo_med", "original" };

    public static async Task<byte[]> DownloadBestAsync(HttpClient httpClient, string imageId,
                                                         IReadOnlyList<string> sizeCandidates)
    {
        byte[] best     = null;
        long   bestArea = -1;

        foreach(string size in sizeCandidates)
        {
            byte[] candidate;

            try
            {
                candidate = await httpClient.GetByteArrayAsync(
                    $"https://images.igdb.com/igdb/image/upload/t_{size}/{imageId}.jpg");
            }
            catch(HttpRequestException)
            {
                continue;
            }

            (int width, int height) = IdentifyDimensions(candidate);
            long area = (long)width * height;

            if(area > bestArea)
            {
                best     = candidate;
                bestArea = area;
            }
        }

        return best;
    }

    static (int width, int height) IdentifyDimensions(byte[] bytes)
    {
        string tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.jpg");

        try
        {
            File.WriteAllBytes(tempPath, bytes);

            var p = new Process
            {
                StartInfo =
                {
                    FileName               = "magick",
                    CreateNoWindow         = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError  = true
                }
            };
            p.StartInfo.ArgumentList.Add("identify");
            p.StartInfo.ArgumentList.Add("-format");
            p.StartInfo.ArgumentList.Add("%w %h\n");
            p.StartInfo.ArgumentList.Add($"{tempPath}[0]");
            p.StartInfo.Environment["MAGICK_THREAD_LIMIT"] = "1";

            p.Start();

            Task<string> stderrTask = p.StandardError.ReadToEndAsync();
            Task<string> stdoutTask = p.StandardOutput.ReadToEndAsync();
            p.WaitForExit();
            string stdout = stdoutTask.GetAwaiter().GetResult();
            _ = stderrTask.GetAwaiter().GetResult();

            if(p.ExitCode != 0) return (0, 0);

            string firstLine = stdout.Split('\n', 2)[0].Trim();
            string[] parts   = firstLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if(parts.Length < 2) return (0, 0);

            if(!int.TryParse(parts[0], out int w) || !int.TryParse(parts[1], out int h)) return (0, 0);

            return (w, h);
        }
        catch(Exception)
        {
            return (0, 0);
        }
        finally
        {
            try
            {
                File.Delete(tempPath);
            }
            catch(IOException) { }
        }
    }
}
