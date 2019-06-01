using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Cicm.Database;
using Cicm.Database.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.MetaData;
using SixLabors.ImageSharp.MetaData.Profiles.Exif;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Primitives;
using SkiaSharp;

namespace cicm_web.Helpers
{
    public class Photos
    {
        public static MachinePhoto Add(string tmpFile, Guid newId, string webRootPath, string contentRootPath,
                                       string extension)
        {
            Image<Rgba32> image = Image.Load(tmpFile);

            MachinePhoto photo = new MachinePhoto();

            foreach(ImageProperty prop in image.MetaData.Properties)
                switch(prop.Name)
                {
                    case "aux:Lens":
                        photo.Lens = prop.Value;
                        break;
                }

            foreach(ExifValue exif in image.MetaData.ExifProfile.Values.ToList())
                switch(exif.Tag)
                {
                    case ExifTag.Artist:
                        photo.Author = exif.Value as string;
                        break;
                    case ExifTag.Make:
                        photo.CameraManufacturer = exif.Value as string;
                        break;
                    case ExifTag.ColorSpace:
                        photo.ColorSpace = (ColorSpace)exif.Value;
                        break;
                    case ExifTag.UserComment:
                        photo.Comments = Encoding.ASCII.GetString(exif.Value as byte[]);
                        break;
                    case ExifTag.Contrast:
                        photo.Contrast = (Contrast)exif.Value;
                        break;
                    case ExifTag.DateTimeDigitized:
                        photo.CreationDate =
                            DateTime.ParseExact(exif.Value.ToString(), "yyyy:MM:dd HH:mm:ss",
                                                CultureInfo.InvariantCulture);
                        break;
                    case ExifTag.DigitalZoomRatio:
                        photo.DigitalZoomRatio = ((Rational)exif.Value).ToDouble();
                        break;
                    case ExifTag.ExifVersion:
                        photo.ExifVersion = Encoding.ASCII.GetString(exif.Value as byte[]);
                        break;
                    case ExifTag.ExposureTime:
                    {
                        Rational rat = (Rational)exif.Value;
                        photo.Exposure = rat.Denominator == 1 ? rat.Numerator.ToString() : rat.ToString();

                        break;
                    }

                    case ExifTag.ExposureMode:
                        photo.ExposureMethod = (ExposureMode)exif.Value;
                        break;
                    case ExifTag.ExposureProgram:
                        photo.ExposureProgram = (ExposureProgram)exif.Value;
                        break;
                    case ExifTag.Flash:
                        photo.Flash = (Flash)exif.Value;
                        break;
                    case ExifTag.FNumber:
                        photo.Focal = ((Rational)exif.Value).ToDouble();
                        break;
                    case ExifTag.FocalLength:
                        photo.FocalLength = ((Rational)exif.Value).ToDouble();
                        break;
                    case ExifTag.FocalLengthIn35mmFilm:
                        photo.FocalLengthEquivalent = exif.Value as ushort?;
                        break;
                    case ExifTag.XResolution:
                        photo.HorizontalResolution = ((Rational)exif.Value).ToDouble();
                        break;
                    case ExifTag.ISOSpeedRatings:
                        photo.IsoRating = (ushort)exif.Value;
                        break;
                    case ExifTag.LensModel:
                        photo.Lens = exif.Value as string;
                        break;
                    case ExifTag.LightSource:
                        photo.LightSource = (LightSource)exif.Value;
                        break;
                    case ExifTag.MeteringMode:
                        photo.MeteringMode = (MeteringMode)exif.Value;
                        break;
                    case ExifTag.ResolutionUnit:
                        photo.ResolutionUnit = (ResolutionUnit)exif.Value;
                        break;
                    case ExifTag.Orientation:
                        photo.Orientation = (Orientation)exif.Value;
                        break;
                    case ExifTag.Saturation:
                        photo.Saturation = (Saturation)exif.Value;
                        break;
                    case ExifTag.SceneCaptureType:
                        photo.SceneCaptureType = (SceneCaptureType)exif.Value;
                        break;
                    case ExifTag.SensingMethod:
                        photo.SensingMethod = (SensingMethod)exif.Value;
                        break;
                    case ExifTag.Software:
                        photo.SoftwareUsed = exif.Value as string;
                        break;
                    case ExifTag.SubjectDistanceRange:
                        photo.SubjectDistanceRange = (SubjectDistanceRange)exif.Value;
                        break;
                    case ExifTag.YResolution:
                        photo.VerticalResolution = ((Rational)exif.Value).ToDouble();
                        break;
                    case ExifTag.WhiteBalance:
                        photo.WhiteBalance = (WhiteBalance)exif.Value;
                        break;
                    default:
                        image.MetaData.ExifProfile.RemoveValue(exif.Tag);
                        break;
                }

            if(!Directory.Exists(Path.Combine(webRootPath,          "assets", "photos")))
                Directory.CreateDirectory(Path.Combine(webRootPath, "assets", "photos"));
            if(!Directory.Exists(Path.Combine(webRootPath,          "assets", "photos", "machines")))
                Directory.CreateDirectory(Path.Combine(webRootPath, "assets", "photos", "machines"));
            if(!Directory.Exists(Path.Combine(webRootPath,          "assets", "photos", "machines", "thumbs")))
                Directory.CreateDirectory(Path.Combine(webRootPath, "assets", "photos", "machines", "thumbs"));

            string outJpeg = Path.Combine(webRootPath, "assets", "photos", "machines", newId + ".jpg");

            using(FileStream jpegStream =
                new FileStream(outJpeg, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None))
                image.SaveAsJpeg(jpegStream);

            int imgMax = Math.Max(image.Width, image.Height);
            int width  = image.Width;
            int height = image.Height;

            image.Dispose();

            SKBitmap skBitmap = SKBitmap.Decode(outJpeg);

            foreach(string format in new[] {"jpg", "webp"})
            {
                if(!Directory.Exists(Path.Combine(webRootPath, "assets/photos/machines/thumbs", format))) ;
                Directory.CreateDirectory(Path.Combine(webRootPath, "assets/photos/machines/thumbs", format));

                SKEncodedImageFormat skFormat;
                switch(format)
                {
                    case "webp":
                        skFormat = SKEncodedImageFormat.Webp;
                        break;
                    default:
                        skFormat = SKEncodedImageFormat.Jpeg;
                        break;
                }

                foreach(int multiplier in new[] {1, 2, 3})
                {
                    if(!Directory.Exists(Path.Combine(webRootPath, "assets/photos/machines/thumbs", format,
                                                      $"{multiplier}x"))) ;
                    Directory.CreateDirectory(Path.Combine(webRootPath, "assets/photos/machines/thumbs", format,
                                                           $"{multiplier}x"));

                    string resized = Path.Combine(webRootPath, "assets/photos/machines/thumbs", format,
                                                  $"{multiplier}x", newId + $".{format}");

                    if(File.Exists(resized)) continue;

                    float canvasMin = 256 * multiplier;

                    float scale = canvasMin / imgMax;

                    // Do not enlarge images
                    if(scale > 1) scale = 1;

                    SKBitmap skResized = skBitmap.Resize(new SKImageInfo((int)(width * scale), (int)(height * scale)),
                                                         SKFilterQuality.High);
                    SKImage    skImage = SKImage.FromBitmap(skResized);
                    SKData     data    = skImage.Encode(skFormat, 100);
                    FileStream outfs   = new FileStream(resized, FileMode.CreateNew);
                    data.SaveTo(outfs);
                    outfs.Close();
                }
            }

            if(!Directory.Exists(Path.Combine(contentRootPath,          "originals", "photos")))
                Directory.CreateDirectory(Path.Combine(contentRootPath, "originals", "photos"));
            if(!Directory.Exists(Path.Combine(contentRootPath,          "originals", "photos")))
                Directory.CreateDirectory(Path.Combine(contentRootPath, "originals", "photos"));
            if(!Directory.Exists(Path.Combine(contentRootPath,          "originals", "photos", "machines")))
                Directory.CreateDirectory(Path.Combine(contentRootPath, "originals", "photos", "machines"));

            File.Move(tmpFile, Path.Combine(contentRootPath, "originals", "photos", "machines", newId + extension));

            return photo;
        }
    }
}