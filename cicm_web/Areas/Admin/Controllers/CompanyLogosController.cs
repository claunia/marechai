using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Marechai.Database.Models;
using Marechai.Areas.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using SkiaSharp;
using SKSvg = SkiaSharp.Extended.Svg.SKSvg;

namespace Marechai.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class CompanyLogosController : Controller
    {
        readonly cicmContext         _context;
        readonly IHostingEnvironment hostingEnvironment;

        public CompanyLogosController(cicmContext context, IHostingEnvironment env)
        {
            _context           = context;
            hostingEnvironment = env;
        }

        // GET: CompanyLogos
        public async Task<IActionResult> Index()
        {
            IIncludableQueryable<CompanyLogo, Company> cicmContext = _context.CompanyLogos.Include(c => c.Company);
            return View(await cicmContext.OrderBy(l => l.Company.Name).ThenBy(l => l.Year)
                                         .Select(l => new CompanyLogoViewModel
                                          {
                                              Company = l.Company.Name, Id = l.Id, Year = l.Year
                                          }).ToListAsync());
        }

        // GET: CompanyLogos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if(id == null) return NotFound();

            CompanyLogo companyLogo = await _context.CompanyLogos
                                                    .Include(c => c.Company).FirstOrDefaultAsync(m => m.Id == id);
            if(companyLogo == null) return NotFound();

            return View(companyLogo);
        }

        // GET: CompanyLogos/Create
        // TODO: Upload
        public IActionResult Create()
        {
            ViewData["CompanyId"] =
                new
                    SelectList(_context.Companies.Select(c => new CompanyViewModel {Name = c.Name, Id = c.Id}).OrderBy(c => c.Name),
                               "Id", "Name");
            return View();
        }

        // POST: CompanyLogos/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        // TODO: Upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CompanyId,Year,SvgLogo")] CompanyLogo companyLogo)
        {
            if(!ModelState.IsValid)
            {
                ViewData["CompanyId"] =
                    new
                        SelectList(_context.Companies.Select(c => new CompanyViewModel {Name = c.Name, Id = c.Id}).OrderBy(c => c.Name),
                                   "Id", "Name", companyLogo.CompanyId);
                return View(companyLogo);
            }

            using(MemoryStream svgMs = new MemoryStream())
            {
                await companyLogo.SvgLogo.CopyToAsync(svgMs);

                svgMs.Position = 0;

                try
                {
                    StreamReader sr     = new StreamReader(svgMs, Encoding.UTF8);
                    string       svgStr = await sr.ReadToEndAsync();
                    XmlDocument  xml    = new XmlDocument();
                    xml.LoadXml(svgStr);
                }
                catch(XmlException e)
                {
                    companyLogo.SvgLogo      = null;
                    companyLogo.ErrorMessage = "Not a valid SVG file.";

                    ViewData["CompanyId"] =
                        new
                            SelectList(_context.Companies.Select(c => new CompanyViewModel {Name = c.Name, Id = c.Id}).OrderBy(c => c.Name),
                                       "Id", "Name", companyLogo.CompanyId);
                    return View(companyLogo);
                }

                svgMs.Position   = 0;
                companyLogo.Guid = Guid.NewGuid();

                string vectorial = Path.Combine(hostingEnvironment.WebRootPath, "assets/logos",
                                                companyLogo.Guid + ".svg");
                if(System.IO.File.Exists(vectorial))
                {
                    companyLogo.SvgLogo      = null;
                    companyLogo.ErrorMessage = "GUID clash, please retry and report to the administrator.";

                    ViewData["CompanyId"] =
                        new
                            SelectList(_context.Companies.Select(c => new CompanyViewModel {Name = c.Name, Id = c.Id}).OrderBy(c => c.Name),
                                       "Id", "Name", companyLogo.CompanyId);
                    return View(companyLogo);
                }

                FileStream outSvg = new FileStream(vectorial, FileMode.CreateNew);
                await svgMs.CopyToAsync(outSvg);
                svgMs.Position = 0;

                SKSvg svg = null;

                try
                {
                    foreach(string format in new[] {"png", "webp"})
                    {
                        if(!Directory.Exists(Path.Combine(hostingEnvironment.WebRootPath, "assets/logos", format))) ;
                        Directory.CreateDirectory(Path.Combine(hostingEnvironment.WebRootPath, "assets/logos", format));

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
                            if(!Directory.Exists(Path.Combine(hostingEnvironment.WebRootPath, "assets/logos", format,
                                                              $"{multiplier}x"))) ;
                            Directory.CreateDirectory(Path.Combine(hostingEnvironment.WebRootPath, "assets/logos",
                                                                   format, $"{multiplier}x"));

                            string rendered = Path.Combine(hostingEnvironment.WebRootPath, "assets/logos", format,
                                                           $"{multiplier}x", companyLogo.Guid + $".{format}");

                            if(System.IO.File.Exists(rendered))
                            {
                                companyLogo.SvgLogo      = null;
                                companyLogo.ErrorMessage = "GUID clash, please retry and report to the administrator.";

                                ViewData["CompanyId"] =
                                    new
                                        SelectList(_context.Companies.Select(c => new CompanyViewModel {Name = c.Name, Id = c.Id}).OrderBy(c => c.Name),
                                                   "Id", "Name", companyLogo.CompanyId);
                                return View(companyLogo);
                            }

                            Console.WriteLine("Rendering {0}", rendered);
                            if(svg == null)
                            {
                                svg = new SKSvg();
                                svg.Load(svgMs);
                            }

                            SKRect   svgSize = svg.Picture.CullRect;
                            SKMatrix matrix  = SKMatrix.MakeScale(multiplier, multiplier);
                            SKBitmap bitmap = new SKBitmap((int)(svgSize.Width  * multiplier),
                                                           (int)(svgSize.Height * multiplier));
                            SKCanvas canvas = new SKCanvas(bitmap);
                            canvas.DrawPicture(svg.Picture, ref matrix);
                            canvas.Flush();
                            SKImage    image = SKImage.FromBitmap(bitmap);
                            SKData     data  = image.Encode(skFormat, 100);
                            FileStream outfs = new FileStream(rendered, FileMode.CreateNew);
                            data.SaveTo(outfs);
                            outfs.Close();

                            svgMs.Position = 0;
                        }
                    }

                    foreach(string format in new[] {"png", "webp"})
                    {
                        if(!Directory.Exists(Path.Combine(hostingEnvironment.WebRootPath, "assets/logos/thumbs",
                                                          format))) ;
                        Directory.CreateDirectory(Path.Combine(hostingEnvironment.WebRootPath, "assets/logos/thumbs",
                                                               format));

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
                            if(!Directory.Exists(Path.Combine(hostingEnvironment.WebRootPath, "assets/logos/thumbs",
                                                              format, $"{multiplier}x"))) ;
                            Directory.CreateDirectory(Path.Combine(hostingEnvironment.WebRootPath,
                                                                   "assets/logos/thumbs", format, $"{multiplier}x"));

                            string rendered = Path.Combine(hostingEnvironment.WebRootPath, "assets/logos/thumbs",
                                                           format, $"{multiplier}x", companyLogo.Guid + $".{format}");

                            if(System.IO.File.Exists(rendered))
                            {
                                companyLogo.SvgLogo      = null;
                                companyLogo.ErrorMessage = "GUID clash, please retry and report to the administrator.";

                                ViewData["CompanyId"] =
                                    new
                                        SelectList(_context.Companies.Select(c => new CompanyViewModel {Name = c.Name, Id = c.Id}).OrderBy(c => c.Name),
                                                   "Id", "Name", companyLogo.CompanyId);
                                return View(companyLogo);
                            }

                            Console.WriteLine("Rendering {0}", rendered);
                            if(svg == null)
                            {
                                svg = new SKSvg();
                                svg.Load(svgMs);
                            }

                            SKRect   svgSize   = svg.Picture.CullRect;
                            float    svgMax    = Math.Max(svgSize.Width, svgSize.Height);
                            float    canvasMin = 32        * multiplier;
                            float    scale     = canvasMin / svgMax;
                            SKMatrix matrix    = SKMatrix.MakeScale(scale, scale);
                            SKBitmap bitmap =
                                new SKBitmap((int)(svgSize.Width * scale), (int)(svgSize.Height * scale));
                            SKCanvas canvas = new SKCanvas(bitmap);
                            canvas.DrawPicture(svg.Picture, ref matrix);
                            canvas.Flush();
                            SKImage    image = SKImage.FromBitmap(bitmap);
                            SKData     data  = image.Encode(skFormat, 100);
                            FileStream outfs = new FileStream(rendered, FileMode.CreateNew);
                            data.SaveTo(outfs);
                            outfs.Close();

                            svgMs.Position = 0;
                        }
                    }
                }
                catch(IOException e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }

            _context.Add(companyLogo);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: CompanyLogos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if(id == null) return NotFound();

            CompanyLogo companyLogo = await _context.CompanyLogos.FirstOrDefaultAsync(c => c.Id == id);
            if(companyLogo == null) return NotFound();

            ViewData["CompanyId"] =
                new SelectList(_context.Companies.OrderBy(l => l.Name), "Id", "Name", companyLogo.CompanyId);
            return View(companyLogo);
        }

        // POST: CompanyLogos/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CompanyId,Year,Guid")] CompanyLogo companyLogo)
        {
            if(id != companyLogo.Id) return NotFound();

            if(ModelState.IsValid)
            {
                try
                {
                    _context.Update(companyLogo);
                    await _context.SaveChangesAsync();
                }
                catch(DbUpdateConcurrencyException)
                {
                    if(!CompanyLogoExists(companyLogo.Id)) return NotFound();

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["CompanyId"] =
                new SelectList(_context.Companies.OrderBy(l => l.Name), "Id", "Name", companyLogo.CompanyId);
            return View(companyLogo);
        }

        // GET: CompanyLogos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if(id == null) return NotFound();

            CompanyLogo companyLogo = await _context.CompanyLogos
                                                    .Include(c => c.Company).FirstOrDefaultAsync(m => m.Id == id);
            if(companyLogo == null) return NotFound();

            return View(companyLogo);
        }

        // POST: CompanyLogos/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            CompanyLogo companyLogo = await _context.CompanyLogos.FirstOrDefaultAsync(m => m.Id == id);
            if(companyLogo == null) return NotFound();

            _context.CompanyLogos.Remove(companyLogo);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        bool CompanyLogoExists(int id)
        {
            return _context.CompanyLogos.Any(e => e.Id == id);
        }
    }
}