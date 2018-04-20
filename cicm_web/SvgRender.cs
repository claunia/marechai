﻿using System;
using System.IO;
using Cicm.Database.Schemas;
using SkiaSharp;
using Console = System.Console;
using SKSvg = SkiaSharp.Extended.Svg.SKSvg;

namespace cicm_web
{
    public static class SvgRender
    {
        public static void RenderCountries()
        {
            if(!Directory.Exists("wwwroot/assets/flags/countries")) return;

            foreach(string file in Directory.GetFiles("wwwroot/assets/flags/countries/", "*.svg",
                                                      SearchOption.TopDirectoryOnly))
            {
                SKSvg svg = null;

                string flagname = Path.GetFileNameWithoutExtension(file);

                foreach(string format in new[] {"png", "webp"})
                {
                    if(!Directory.Exists(Path.Combine("wwwroot/assets/flags/countries", format))) ;
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

                    foreach(int multiplier in new[] {1, 2, 3})
                    {
                        if(!Directory.Exists(Path.Combine("wwwroot/assets/flags/countries", format, $"{multiplier}x"))
                        ) ;
                        Directory.CreateDirectory(Path.Combine("wwwroot/assets/flags/countries", format,
                                                               $"{multiplier}x"));

                        string rendered = Path.Combine("wwwroot/assets/flags/countries", format, $"{multiplier}x",
                                                       flagname + $".{format}");

                        if(File.Exists(rendered)) continue;

                        Console.WriteLine("Rendering {0}", rendered);
                        if(svg == null)
                        {
                            svg = new SKSvg();
                            svg.Load(file);
                        }

                        SKRect   svgSize   = svg.Picture.CullRect;
                        float    svgMax    = Math.Max(svgSize.Width, svgSize.Height);
                        float    canvasMin = 32        * multiplier;
                        float    scale     = canvasMin / svgMax;
                        SKMatrix matrix    = SKMatrix.MakeScale(scale, scale);
                        SKBitmap bitmap    = new SKBitmap((int)(svgSize.Width * scale), (int)(svgSize.Height * scale));
                        SKCanvas canvas    = new SKCanvas(bitmap);
                        canvas.DrawPicture(svg.Picture, ref matrix);
                        canvas.Flush();
                        SKImage    image = SKImage.FromBitmap(bitmap);
                        SKData     data  = image.Encode(skFormat, 100);
                        FileStream outfs = new FileStream(rendered, FileMode.CreateNew);
                        data.SaveTo(outfs);
                        outfs.Close();
                    }
                }
            }
        }

        public static void ImportCompanyLogos()
        {
            if(!Directory.Exists("wwwroot/assets/incoming")) return;

            foreach(string file in Directory.GetFiles("wwwroot/assets/incoming", "company_*.svg",
                                                      SearchOption.TopDirectoryOnly))
            {
                string filename = Path.GetFileNameWithoutExtension(file);

                if(!filename.StartsWith("company_")) continue;

                string[] pieces = filename.Split('_');

                if(pieces.Length != 3) continue;

                Guid guid = Guid.NewGuid();

                if(!int.TryParse(pieces[1], out int companyId)) continue;

                if(!int.TryParse(pieces[2], out int year)) continue;

                CompanyLogo entry = new CompanyLogo {CompanyId = companyId, Year = year, Guid = guid};

                if(!Program.Database.Operations.AddCompanyLogo(entry, out _)) continue;

                File.Move(file, $"wwwroot/assets/logos/{guid}.svg");
            }
        }
    }
}