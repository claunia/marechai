using System;
using System.IO;
using SkiaSharp;
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

                foreach(string format in new[] {"png", "jpeg", "webp"})
                {
                    if(!Directory.Exists(Path.Combine("wwwroot/assets/flags/countries", format))) ;
                    Directory.CreateDirectory(Path.Combine("wwwroot/assets/flags/countries", format));

                    SKEncodedImageFormat skFormat;
                    switch(format)
                    {
                        case "jpeg":
                            skFormat = SKEncodedImageFormat.Jpeg;
                            break;
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
    }
}