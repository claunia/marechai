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
// Copyright © 2003-2020 Natalia Portillo
*******************************************************************************/

using System;
using System.IO;
using Marechai.Database.Models;
using SkiaSharp;
using Svg.Skia;

namespace Marechai.Helpers
{
    public static class SvgRender
    {
        public static void RenderCountries()
        {
            if(!Directory.Exists("wwwroot/assets/flags/countries"))
                return;

            foreach(string file in Directory.GetFiles("wwwroot/assets/flags/countries/", "*.svg",
                                                      SearchOption.TopDirectoryOnly))
            {
                SKSvg svg = null;

                string flagName = Path.GetFileNameWithoutExtension(file);

                foreach(string format in new[]
                {
                    "png", "webp"
                })
                {
                    if(!Directory.Exists(Path.Combine("wwwroot/assets/flags/countries", format)))
                        Directory.CreateDirectory(Path.Combine("wwwroot/assets/flags/countries", format));

                    SKEncodedImageFormat skFormat;

                    switch(format)
                    {
                        case "webp":
                            skFormat = SKEncodedImageFormat.Webp;

                            break;
                        default:
                            skFormat = SKEncodedImageFormat.Png;

                            break;
                    }

                    foreach(int multiplier in new[]
                    {
                        1, 2, 3
                    })
                    {
                        if(!Directory.Exists(Path.Combine("wwwroot/assets/flags/countries", format, $"{multiplier}x")))
                            Directory.CreateDirectory(Path.Combine("wwwroot/assets/flags/countries", format,
                                                                   $"{multiplier}x"));

                        string rendered = Path.Combine("wwwroot/assets/flags/countries", format, $"{multiplier}x",
                                                       flagName + $".{format}");

                        if(File.Exists(rendered))
                            continue;

                        Console.WriteLine("Rendering {0}", rendered);

                        if(svg == null)
                        {
                            svg = new SKSvg();
                            svg.Load(file);
                        }

                        var outFs = new FileStream(rendered, FileMode.CreateNew);
                        RenderSvg(svg, outFs, skFormat, 32, multiplier);
                        outFs.Close();
                    }
                }
            }
        }

        public static void ImportCompanyLogos(MarechaiContext context)
        {
            if(!Directory.Exists("wwwroot/assets/incoming"))
                return;

            foreach(string file in Directory.GetFiles("wwwroot/assets/incoming", "company_*.svg",
                                                      SearchOption.TopDirectoryOnly))
            {
                string filename = Path.GetFileNameWithoutExtension(file);

                if(!filename.StartsWith("company_", StringComparison.InvariantCulture))
                    continue;

                string[] pieces = filename.Split('_');

                if(pieces.Length != 3)
                    continue;

                var guid = Guid.NewGuid();

                if(!int.TryParse(pieces[1], out int companyId))
                    continue;

                if(!int.TryParse(pieces[2], out int year))
                    continue;

                try
                {
                    context.CompanyLogos.Add(new CompanyLogo
                    {
                        CompanyId = companyId,
                        Year      = year,
                        Guid      = guid
                    });

                    context.SaveChanges();
                }
                catch(Exception)
                {
                    continue;
                }

                SKSvg svg = null;

                foreach(int minSize in new[]
                {
                    256, 32
                })
                {
                    foreach(string format in new[]
                    {
                        "png", "webp"
                    })
                    {
                        string outDir = minSize == 32 ? Path.Combine("wwwroot/assets/logos/thumbs", format)
                                            : Path.Combine("wwwroot/assets/logos", format);

                        if(!Directory.Exists(outDir))
                            Directory.CreateDirectory(outDir);

                        SKEncodedImageFormat skFormat;

                        switch(format)
                        {
                            case "webp":
                                skFormat = SKEncodedImageFormat.Webp;

                                break;
                            default:
                                skFormat = SKEncodedImageFormat.Png;

                                break;
                        }

                        foreach(int multiplier in new[]
                        {
                            1, 2, 3
                        })
                        {
                            string outPath = Path.Combine(outDir, $"{multiplier}x");

                            if(!Directory.Exists(outPath))
                                Directory.CreateDirectory(outPath);

                            string rendered = Path.Combine(outPath, $"{guid}.{format}");

                            if(File.Exists(rendered))
                                continue;

                            Console.WriteLine("Rendering {0}", rendered);

                            if(svg == null)
                            {
                                svg = new SKSvg();
                                svg.Load(file);
                            }

                            var outFs = new FileStream(rendered, FileMode.CreateNew);
                            RenderSvg(svg, outFs, skFormat, minSize, multiplier);
                            outFs.Close();
                        }
                    }
                }

                File.Move(file, $"wwwroot/assets/logos/{guid}.svg");
            }
        }

        public static void RenderSvg(SKSvg svg, Stream outStream, SKEncodedImageFormat skFormat, int minSize,
                                     int multiplier)
        {
            SKRect svgSize   = svg.Picture.CullRect;
            float  svgMax    = Math.Max(svgSize.Width, svgSize.Height);
            float  canvasMin = minSize   * multiplier;
            float  scale     = canvasMin / svgMax;
            var    matrix    = SKMatrix.MakeScale(scale, scale);
            var    bitmap    = new SKBitmap((int)(svgSize.Width * scale), (int)(svgSize.Height * scale));
            var    canvas    = new SKCanvas(bitmap);
            canvas.Clear();
            canvas.DrawPicture(svg.Picture, ref matrix);
            canvas.Flush();
            var    image = SKImage.FromBitmap(bitmap);
            SKData data  = image.Encode(skFormat, 100);
            data.SaveTo(outStream);
        }

        // TODO: Reduce code duplication
        public static void RenderCompanyLogo(Guid guid, Stream svgStream, string wwwroot)
        {
            SKSvg svg = null;

            foreach(int minSize in new[]
            {
                256, 32
            })
            {
                foreach(string format in new[]
                {
                    "png", "webp"
                })
                {
                    string outDir = minSize == 32 ? Path.Combine(wwwroot, "assets/logos/thumbs", format)
                                        : Path.Combine(wwwroot, "assets/logos", format);

                    if(!Directory.Exists(outDir))
                        Directory.CreateDirectory(outDir);

                    SKEncodedImageFormat skFormat;

                    switch(format)
                    {
                        case "webp":
                            skFormat = SKEncodedImageFormat.Webp;

                            break;
                        default:
                            skFormat = SKEncodedImageFormat.Png;

                            break;
                    }

                    foreach(int multiplier in new[]
                    {
                        1, 2, 3
                    })
                    {
                        string outPath = Path.Combine(outDir, $"{multiplier}x");

                        if(!Directory.Exists(outPath))
                            Directory.CreateDirectory(outPath);

                        string rendered = Path.Combine(outPath, $"{guid}.{format}");

                        if(File.Exists(rendered))
                            continue;

                        Console.WriteLine("Rendering {0}", rendered);

                        if(svg == null)
                        {
                            svg = new SKSvg();
                            svg.Load(svgStream);
                        }

                        var outFs = new FileStream(rendered, FileMode.CreateNew);
                        RenderSvg(svg, outFs, skFormat, minSize, multiplier);
                        outFs.Close();
                    }
                }
            }
        }
    }
}