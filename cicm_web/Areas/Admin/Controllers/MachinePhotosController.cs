using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cicm.Database;
using Cicm.Database.Models;
using cicm_web.Areas.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.MetaData;
using SixLabors.ImageSharp.MetaData.Profiles.Exif;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Primitives;
using SkiaSharp;

namespace cicm_web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class MachinePhotosController : Controller
    {
        readonly cicmContext                  _context;
        readonly IHostingEnvironment          hostingEnvironment;
        readonly UserManager<ApplicationUser> userManager;

        public MachinePhotosController(cicmContext                  context, IHostingEnvironment hostingEnvironment,
                                       UserManager<ApplicationUser> userManager)
        {
            _context                = context;
            this.hostingEnvironment = hostingEnvironment;
            this.userManager        = userManager;
        }

        // GET: MachinePhotos
        public async Task<IActionResult> Index()
        {
            return View(await _context.MachinePhotos.Include(m => m.Machine).Include(m => m.Machine.Company)
                                      .Include(m => m.User).Select(p => new MachinePhotoViewModel
                                       {
                                           Id      = p.Id,
                                           Author  = p.Author,
                                           License = p.License.Name,
                                           Machine =
                                               $"{p.Machine.Company.Name} {p.Machine.Name}",
                                           UploadDate = p.UploadDate,
                                           UploadUser = p.User.UserName,
                                           LicenseId  = p.License.Id
                                       }).OrderBy(p => p.Machine)
                                      .ThenBy(p => p.UploadUser).ThenBy(p => p.UploadDate).ToListAsync());
        }

        // GET: MachinePhotos/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if(id == null) return NotFound();

            MachinePhotoDetailsViewModel machinePhoto = await _context
                                                             .MachinePhotos
                                                             .Select(m => new MachinePhotoDetailsViewModel
                                                              {
                                                                  Id                 = m.Id,
                                                                  CameraManufacturer = m.CameraManufacturer,
                                                                  CameraModel        = m.CameraModel,
                                                                  ColorSpace         = m.ColorSpace,
                                                                  Comments           = m.Comments,
                                                                  Contrast           = m.Contrast,
                                                                  CreationDate       = m.CreationDate,
                                                                  DigitalZoomRatio   = m.DigitalZoomRatio,
                                                                  ExifVersion        = m.ExifVersion,
                                                                  Exposure           = m.Exposure,
                                                                  ExposureProgram    = m.ExposureProgram,
                                                                  Flash              = m.Flash,
                                                                  Focal              = m.Focal,
                                                                  FocalLength        = m.FocalLength,
                                                                  FocalLengthEquivalent =
                                                                      m.FocalLengthEquivalent,
                                                                  HorizontalResolution =
                                                                      m.HorizontalResolution,
                                                                  IsoRating        = m.IsoRating,
                                                                  Lens             = m.Lens,
                                                                  LightSource      = m.LightSource,
                                                                  MeteringMode     = m.MeteringMode,
                                                                  ResolutionUnit   = m.ResolutionUnit,
                                                                  Orientation      = m.Orientation,
                                                                  Saturation       = m.Saturation,
                                                                  SceneCaptureType = m.SceneCaptureType,
                                                                  SensingMethod    = m.SensingMethod,
                                                                  Sharpness        = m.Sharpness,
                                                                  SoftwareUsed     = m.SoftwareUsed,
                                                                  SubjectDistanceRange =
                                                                      m.SubjectDistanceRange,
                                                                  UploadDate         = m.UploadDate,
                                                                  VerticalResolution = m.VerticalResolution,
                                                                  WhiteBalance       = m.WhiteBalance,
                                                                  License            = m.License.Name,
                                                                  UploadUser         = m.User.UserName,
                                                                  Machine =
                                                                      $"{m.Machine.Company.Name} {m.Machine.Name}",
                                                                  MachineId = m.Machine.Id
                                                              }).FirstOrDefaultAsync(m => m.Id == id);
            if(machinePhoto == null) return NotFound();

            return View(machinePhoto);
        }

        // GET: MachinePhotos/Create
        public IActionResult Create()
        {
            ViewData["MachineId"] =
                new
                    SelectList(_context.Machines.OrderBy(c => c.Company.Name).ThenBy(c => c.Name).Select(m => new {m.Id, Name = $"{m.Company.Name} {m.Name}"}),
                               "Id", "Name");
            ViewData["LicenseId"] =
                new SelectList(_context.Licenses.OrderBy(c => c.Name).Select(l => new {l.Id, l.Name}), "Id", "Name");
            return View();
        }

        // POST: MachinePhotos/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MachineId,LicenseId,Photo")] MachinePhotoViewModel machinePhoto)
        {
            if(!ModelState.IsValid) return View(machinePhoto);

            Guid   newId       = Guid.NewGuid();
            string tmpPath     = Path.GetTempPath();
            string tmpFileName = newId + ".tmp";
            string tmpFile     = Path.Combine(tmpPath, tmpFileName);

            if(System.IO.File.Exists(tmpFile))
            {
                machinePhoto.ErrorMessage = "Colliding temp file please retry.";
                return View(machinePhoto);
            }

            using(FileStream tmpStream = new FileStream(tmpFile, FileMode.CreateNew))
                await machinePhoto.Photo.CopyToAsync(tmpStream);

            IImageFormat imageinfo = Image.DetectFormat(tmpFile);

            string extension;
            switch(imageinfo?.Name)
            {
                case "JPEG":
                    extension = ".jpg";
                    break;
                case "PNG":
                    extension = ".png";
                    break;
                default:
                    System.IO.File.Delete(tmpFile);
                    machinePhoto.ErrorMessage = "Unsupported file format, only JPEG and PNG are allowed at the moment.";
                    return View(machinePhoto);
            }

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

            if(!Directory.Exists(Path.Combine(hostingEnvironment.WebRootPath,          "assets", "photos")))
                Directory.CreateDirectory(Path.Combine(hostingEnvironment.WebRootPath, "assets", "photos"));
            if(!Directory.Exists(Path.Combine(hostingEnvironment.WebRootPath,          "assets", "photos", "machines")))
                Directory.CreateDirectory(Path.Combine(hostingEnvironment.WebRootPath, "assets", "photos", "machines"));
            if(!Directory.Exists(Path.Combine(hostingEnvironment.WebRootPath, "assets", "photos", "machines", "thumbs"))
            )
                Directory.CreateDirectory(Path.Combine(hostingEnvironment.WebRootPath, "assets", "photos", "machines",
                                                       "thumbs"));

            string outJpeg = Path.Combine(hostingEnvironment.WebRootPath, "assets", "photos", "machines",
                                          newId + ".jpg");

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
                if(!Directory.Exists(Path.Combine(hostingEnvironment.WebRootPath, "assets/photos/machines/thumbs",
                                                  format))) ;
                Directory.CreateDirectory(Path.Combine(hostingEnvironment.WebRootPath, "assets/photos/machines/thumbs",
                                                       format));

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
                    if(!Directory.Exists(Path.Combine(hostingEnvironment.WebRootPath, "assets/photos/machines/thumbs",
                                                      format, $"{multiplier}x"))) ;
                    Directory.CreateDirectory(Path.Combine(hostingEnvironment.WebRootPath,
                                                           "assets/photos/machines/thumbs", format, $"{multiplier}x"));

                    string resized = Path.Combine(hostingEnvironment.WebRootPath, "assets/photos/machines/thumbs",
                                                  format, $"{multiplier}x", newId + $".{format}");

                    if(System.IO.File.Exists(resized)) continue;

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

            if(!Directory.Exists(Path.Combine(hostingEnvironment.ContentRootPath,          "originals", "photos")))
                Directory.CreateDirectory(Path.Combine(hostingEnvironment.ContentRootPath, "originals", "photos"));
            if(!Directory.Exists(Path.Combine(hostingEnvironment.ContentRootPath,          "originals", "photos")))
                Directory.CreateDirectory(Path.Combine(hostingEnvironment.ContentRootPath, "originals", "photos"));
            if(!Directory.Exists(Path.Combine(hostingEnvironment.ContentRootPath, "originals", "photos", "machines")))
                Directory.CreateDirectory(Path.Combine(hostingEnvironment.ContentRootPath, "originals", "photos",
                                                       "machines"));

            System.IO.File.Move(tmpFile,
                                Path.Combine(hostingEnvironment.ContentRootPath, "originals", "photos", "machines",
                                             newId + extension));

            photo.Id      = newId;
            photo.User    = await userManager.GetUserAsync(HttpContext.User);
            photo.License = await _context.Licenses.FindAsync(machinePhoto.LicenseId);
            photo.Machine = await _context.Machines.FindAsync(machinePhoto.MachineId);

            _context.Add(photo);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new {Id = newId});
        }

        // GET: MachinePhotos/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if(id == null) return NotFound();

            MachinePhoto machinePhoto = await _context.MachinePhotos.FindAsync(id);
            if(machinePhoto == null) return NotFound();

            ViewData["MachineId"] =
                new
                    SelectList(_context.Machines.OrderBy(m => m.Company.Name).ThenBy(m => m.Name).Select(m => new {m.Id, Name = $"{m.Company.Name} {m.Name}"}),
                               "Id", "Name", machinePhoto.MachineId);
            ViewData["LicenseId"] =
                new SelectList(_context.Licenses.OrderBy(l => l.Name).Select(l => new {l.Id, l.Name}), "Id", "Name",
                               machinePhoto.LicenseId);

            return View(machinePhoto);
        }

        // POST: MachinePhotos/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind(
                                                  "Author,CameraManufacturer,CameraModel,ColorSpace,Comments,Contrast,CreationDate,DigitalZoomRatio,ExifVersion,Exposure,ExposureMethod,ExposureProgram,Flash,Focal,FocalLength,FocalLengthEquivalent,HorizontalResolution,IsoRating,Lens,LicenseId,LightSource,MachineId,MeteringMode,ResolutionUnit,Orientation,Saturation,SceneCaptureType,SensingMethod,Sharpness,SoftwareUsed,SubjectDistanceRange,VerticalResolution,WhiteBalance,Id")]
                                              MachinePhoto machinePhoto)
        {
            if(id != machinePhoto.Id) return NotFound();

            if(!ModelState.IsValid) return View(machinePhoto);

            try
            {
                _context.Update(machinePhoto);
                await _context.SaveChangesAsync();
            }
            catch(DbUpdateConcurrencyException)
            {
                if(!MachinePhotoExists(machinePhoto.Id)) return NotFound();

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: MachinePhotos/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if(id == null) return NotFound();

            MachinePhoto machinePhoto = await _context.MachinePhotos.FirstOrDefaultAsync(m => m.Id == id);
            if(machinePhoto == null) return NotFound();

            return View(machinePhoto);
        }

        // POST: MachinePhotos/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            MachinePhoto machinePhoto = await _context.MachinePhotos.FindAsync(id);
            _context.MachinePhotos.Remove(machinePhoto);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        bool MachinePhotoExists(Guid id)
        {
            return _context.MachinePhotos.Any(e => e.Id == id);
        }
    }
}