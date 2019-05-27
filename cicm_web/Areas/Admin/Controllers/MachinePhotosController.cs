using System;
using System.Linq;
using System.Threading.Tasks;
using Cicm.Database.Models;
using cicm_web.Areas.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cicm_web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class MachinePhotosController : Controller
    {
        readonly cicmContext _context;

        public MachinePhotosController(cicmContext context)
        {
            _context = context;
        }

        // GET: MachinePhotos
        public async Task<IActionResult> Index()
        {
            return View(await _context.MachinePhotos.Include(m => m.Machine).Include(m => m.Machine.Company)
                                      .Include(m => m.User).Select(p => new MachinePhotoViewModel
                                       {
                                           Id      = p.Id,
                                           Author  = p.Author,
                                           License = p.License,
                                           Machine =
                                               $"{p.Machine.Company.Name} {p.Machine.Name}",
                                           UploadDate = p.UploadDate,
                                           UploadUser = p.User.UserName
                                       }).OrderBy(p => p.Machine)
                                      .ThenBy(p => p.UploadUser).ThenBy(p => p.UploadDate).ToListAsync());
        }

        // GET: MachinePhotos/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if(id == null) return NotFound();

            MachinePhoto machinePhoto = await _context.MachinePhotos.FirstOrDefaultAsync(m => m.Id == id);
            if(machinePhoto == null) return NotFound();

            return View(machinePhoto);
        }

        // GET: MachinePhotos/Create
        public IActionResult Create() => View();

        // POST: MachinePhotos/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind(
                "Author,CameraManufacturer,CameraModel,ColorSpace,Comments,Contrast,CreationDate,DigitalZoomRatio,ExifVersion,Exposure,ExposureMethod,ExposureProgram,Flash,Focal,FocalLength,FocalLengthEquivalent,HorizontalResolution,IsoRating,Lens,License,LightSource,MeteringMode,Orientation,PixelComposition,Saturation,SceneCaptureType,SceneControl,SensingMethod,Sharpness,SoftwareUsed,SubjectDistanceRange,UploadDate,VerticalResolution,WhiteBalance,Id")]
            MachinePhoto machinePhoto)
        {
            if(ModelState.IsValid)
            {
                machinePhoto.Id = Guid.NewGuid();
                _context.Add(machinePhoto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(machinePhoto);
        }

        // GET: MachinePhotos/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if(id == null) return NotFound();

            MachinePhoto machinePhoto = await _context.MachinePhotos.FindAsync(id);
            if(machinePhoto == null) return NotFound();

            return View(machinePhoto);
        }

        // POST: MachinePhotos/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind(
                                                  "Author,CameraManufacturer,CameraModel,ColorSpace,Comments,Contrast,CreationDate,DigitalZoomRatio,ExifVersion,Exposure,ExposureMethod,ExposureProgram,Flash,Focal,FocalLength,FocalLengthEquivalent,HorizontalResolution,IsoRating,Lens,License,LightSource,MeteringMode,Orientation,PixelComposition,Saturation,SceneCaptureType,SceneControl,SensingMethod,Sharpness,SoftwareUsed,SubjectDistanceRange,UploadDate,VerticalResolution,WhiteBalance,Id")]
                                              MachinePhoto machinePhoto)
        {
            if(id != machinePhoto.Id) return NotFound();

            if(ModelState.IsValid)
            {
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

            return View(machinePhoto);
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