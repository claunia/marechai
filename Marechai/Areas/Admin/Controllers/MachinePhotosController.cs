using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Areas.Admin.Models;
using Marechai.Database.Models;
using Marechai.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;

namespace Marechai.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize]
    public class MachinePhotosController : Controller
    {
        readonly MarechaiContext              _context;
        readonly IWebHostEnvironment          hostingEnvironment;
        readonly UserManager<ApplicationUser> userManager;

        public MachinePhotosController(MarechaiContext context, IWebHostEnvironment hostingEnvironment,
                                       UserManager<ApplicationUser> userManager)
        {
            _context                = context;
            this.hostingEnvironment = hostingEnvironment;
            this.userManager        = userManager;
        }

        // GET: MachinePhotos
        public async Task<IActionResult> Index() => View(await _context.
                                                               MachinePhotos.Include(m => m.Machine).
                                                               Include(m => m.Machine.Company).Include(m => m.User).
                                                               Select(p => new MachinePhotoViewModel
                                                               {
                                                                   Id      = p.Id, Author = p.Author,
                                                                   License = p.License.Name,
                                                                   Machine =
                                                                       $"{p.Machine.Company.Name} {p.Machine.Name}",
                                                                   UploadDate = p.UploadDate,
                                                                   UploadUser = p.User.UserName,
                                                                   LicenseId  = p.License.Id
                                                               }).OrderBy(p => p.Machine).ThenBy(p => p.UploadUser).
                                                               ThenBy(p => p.UploadDate).ToListAsync());

        // GET: MachinePhotos/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if(id == null)
                return NotFound();

            MachinePhotoDetailsViewModel machinePhoto =
                await _context.MachinePhotos.Select(m => new MachinePhotoDetailsViewModel
                {
                    Id = m.Id, CameraManufacturer = m.CameraManufacturer, CameraModel = m.CameraModel,
                    ColorSpace = m.ColorSpace, Comments = m.Comments, Contrast = m.Contrast,
                    CreationDate = m.CreationDate, DigitalZoomRatio = m.DigitalZoomRatio, ExifVersion = m.ExifVersion,
                    Exposure = m.Exposure, ExposureProgram = m.ExposureProgram, Flash = m.Flash, Focal = m.Focal,
                    FocalLength = m.FocalLength, FocalLengthEquivalent = m.FocalLengthEquivalent,
                    HorizontalResolution = m.HorizontalResolution, IsoRating = m.IsoRating, Lens = m.Lens,
                    LightSource = m.LightSource, MeteringMode = m.MeteringMode, ResolutionUnit = m.ResolutionUnit,
                    Orientation = m.Orientation, Saturation = m.Saturation, SceneCaptureType = m.SceneCaptureType,
                    SensingMethod = m.SensingMethod, Sharpness = m.Sharpness, SoftwareUsed = m.SoftwareUsed,
                    SubjectDistanceRange = m.SubjectDistanceRange, UploadDate = m.UploadDate,
                    VerticalResolution = m.VerticalResolution, WhiteBalance = m.WhiteBalance, License = m.License.Name,
                    UploadUser = m.User.UserName, Machine = $"{m.Machine.Company.Name} {m.Machine.Name}",
                    MachineId = m.Machine.Id, Source = m.Source
                }).FirstOrDefaultAsync(m => m.Id == id);

            if(machinePhoto == null)
                return NotFound();

            return View(machinePhoto);
        }

        // GET: MachinePhotos/Create
        public IActionResult Create()
        {
            ViewData["MachineId"] = new SelectList(_context.Machines.OrderBy(c => c.Company.Name).ThenBy(c => c.Name).
                                                            Select(m => new
                                                            {
                                                                m.Id, Name = $"{m.Company.Name} {m.Name}"
                                                            }), "Id", "Name");

            ViewData["LicenseId"] = new SelectList(_context.Licenses.OrderBy(c => c.Name).Select(l => new
            {
                l.Id, l.Name
            }), "Id", "Name");

            return View();
        }

        // POST: MachinePhotos/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MachineId,LicenseId,Photo,Source")]
                                                MachinePhotoViewModel machinePhoto)
        {
            if(!ModelState.IsValid)
                return View(machinePhoto);

            var    newId       = Guid.NewGuid();
            string tmpPath     = Path.GetTempPath();
            string tmpFileName = newId + ".tmp";
            string tmpFile     = Path.Combine(tmpPath, tmpFileName);

            if(System.IO.File.Exists(tmpFile))
            {
                machinePhoto.ErrorMessage = "Colliding temp file please retry.";

                return View(machinePhoto);
            }

            using(var tmpStream = new FileStream(tmpFile, FileMode.CreateNew))
                await machinePhoto.Photo.CopyToAsync(tmpStream);

            IImageFormat imageinfo = Image.DetectFormat(tmpFile);

            string extension;

            switch(imageinfo?.Name)
            {
                case"JPEG":
                    extension = ".jpg";

                    break;
                case"PNG":
                    extension = ".png";

                    break;
                default:
                    System.IO.File.Delete(tmpFile);
                    machinePhoto.ErrorMessage = "Unsupported file format, only JPEG and PNG are allowed at the moment.";

                    return View(machinePhoto);
            }

            MachinePhoto photo = Photos.Add(tmpFile, newId, hostingEnvironment.WebRootPath,
                                            hostingEnvironment.ContentRootPath, extension);

            photo.Id      = newId;
            photo.User    = await userManager.GetUserAsync(HttpContext.User);
            photo.License = await _context.Licenses.FindAsync(machinePhoto.LicenseId);
            photo.Machine = await _context.Machines.FindAsync(machinePhoto.MachineId);

            _context.Add(photo);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new
            {
                Id = newId
            });
        }

        // GET: MachinePhotos/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if(id == null)
                return NotFound();

            MachinePhoto machinePhoto = await _context.MachinePhotos.FindAsync(id);

            if(machinePhoto == null)
                return NotFound();

            ViewData["MachineId"] = new SelectList(_context.Machines.OrderBy(m => m.Company.Name).ThenBy(m => m.Name).
                                                            Select(m => new
                                                            {
                                                                m.Id, Name = $"{m.Company.Name} {m.Name}"
                                                            }), "Id", "Name", machinePhoto.MachineId);

            ViewData["LicenseId"] = new SelectList(_context.Licenses.OrderBy(l => l.Name).Select(l => new
            {
                l.Id, l.Name
            }), "Id", "Name", machinePhoto.LicenseId);

            return View(machinePhoto);
        }

        // POST: MachinePhotos/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            Guid id,
            [Bind("Author,CameraManufacturer,CameraModel,ColorSpace,Comments,Contrast,CreationDate,DigitalZoomRatio,ExifVersion,Exposure,ExposureMethod,ExposureProgram,Flash,Focal,FocalLength,FocalLengthEquivalent,HorizontalResolution,IsoRating,Lens,LicenseId,LightSource,MachineId,MeteringMode,ResolutionUnit,Orientation,Saturation,SceneCaptureType,SensingMethod,Sharpness,SoftwareUsed,SubjectDistanceRange,VerticalResolution,WhiteBalance,Id,Source")]
            MachinePhoto machinePhoto)
        {
            if(id != machinePhoto.Id)
                return NotFound();

            if(!ModelState.IsValid)
                return View(machinePhoto);

            try
            {
                _context.Update(machinePhoto);
                await _context.SaveChangesAsync();
            }
            catch(DbUpdateConcurrencyException)
            {
                if(!MachinePhotoExists(machinePhoto.Id))
                    return NotFound();

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: MachinePhotos/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if(id == null)
                return NotFound();

            MachinePhotoViewModel machinePhoto =
                await _context.MachinePhotos.Include(m => m.Machine).Include(m => m.Machine.Company).
                               Include(m => m.User).Select(p => new MachinePhotoViewModel
                               {
                                   Id         = p.Id, Author = p.Author, License = p.License.Name,
                                   Machine    = $"{p.Machine.Company.Name} {p.Machine.Name}", UploadDate = p.UploadDate,
                                   UploadUser = p.User.UserName, LicenseId = p.License.Id
                               }).OrderBy(p => p.Machine).ThenBy(p => p.UploadUser).ThenBy(p => p.UploadDate).
                               FirstOrDefaultAsync(m => m.Id == id);

            if(machinePhoto == null)
                return NotFound();

            return View(machinePhoto);
        }

        // POST: MachinePhotos/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            MachinePhoto machinePhoto = await _context.MachinePhotos.FindAsync(id);
            _context.MachinePhotos.Remove(machinePhoto);
            await _context.SaveChangesAsync();

            foreach(string format in new[]
            {
                "jpg", "webp"
            })
            {
                foreach(int multiplier in new[]
                {
                    1, 2, 3
                })
                    System.IO.File.Delete(Path.Combine(hostingEnvironment.WebRootPath, "assets/photos/machines/thumbs",
                                                       format, $"{multiplier}x", id + $".{format}"));
            }

            return RedirectToAction(nameof(Index));
        }

        bool MachinePhotoExists(Guid id) => _context.MachinePhotos.Any(e => e.Id == id);
    }
}