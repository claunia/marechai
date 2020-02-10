using System.Linq;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Marechai.Areas.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Marechai.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class InstructionSetExtensionsByProcessorController : Controller
    {
        readonly MarechaiContext _context;

        public InstructionSetExtensionsByProcessorController(MarechaiContext context)
        {
            _context = context;
        }

        // GET: InstructionSetExtensionsByProcessor
        public async Task<IActionResult> Index()
        {
            IIncludableQueryable<InstructionSetExtensionsByProcessor, Processor> marechaiContext =
                _context.InstructionSetExtensionsByProcessor.Include(i => i.Extension).Include(i => i.Processor);
            return View(await marechaiContext.OrderBy(e => e.Processor.Name).ThenBy(e => e.Extension.Extension)
                                         .Select(e => new InstructionSetExtensionsByProcessorViewModel
                                          {
                                              Id        = e.Id,
                                              Extension = e.Extension.Extension,
                                              Processor = e.Processor.Name
                                          }).ToListAsync());
        }

        // GET: InstructionSetExtensionsByProcessor/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if(id == null) return NotFound();

            InstructionSetExtensionsByProcessor instructionSetExtensionsByProcessor =
                await _context.InstructionSetExtensionsByProcessor.Include(i => i.Extension).Include(i => i.Processor)
                              .FirstOrDefaultAsync(m => m.Id == id);
            if(instructionSetExtensionsByProcessor == null) return NotFound();

            return View(instructionSetExtensionsByProcessor);
        }

        // GET: InstructionSetExtensionsByProcessor/Create
        public IActionResult Create()
        {
            ViewData["ExtensionId"] = new SelectList(_context.InstructionSetExtensions, "Id", "Extension");
            ViewData["ProcessorId"] = new SelectList(_context.Processors,               "Id", "Name");
            return View();
        }

        // POST: InstructionSetExtensionsByProcessor/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Id,ProcessorId,ExtensionId")]
            InstructionSetExtensionsByProcessor instructionSetExtensionsByProcessor)
        {
            if(ModelState.IsValid)
            {
                _context.Add(instructionSetExtensionsByProcessor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["ExtensionId"] = new SelectList(_context.InstructionSetExtensions, "Id", "Extension",
                                                     instructionSetExtensionsByProcessor.ExtensionId);
            ViewData["ProcessorId"] = new SelectList(_context.Processors, "Id", "Name",
                                                     instructionSetExtensionsByProcessor.ProcessorId);
            return View(instructionSetExtensionsByProcessor);
        }

        // GET: InstructionSetExtensionsByProcessor/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if(id == null) return NotFound();

            InstructionSetExtensionsByProcessor instructionSetExtensionsByProcessor =
                await _context.InstructionSetExtensionsByProcessor.FirstOrDefaultAsync(e => e.Id == id);
            if(instructionSetExtensionsByProcessor == null) return NotFound();

            ViewData["ExtensionId"] = new SelectList(_context.InstructionSetExtensions, "Id", "Extension",
                                                     instructionSetExtensionsByProcessor.ExtensionId);
            ViewData["ProcessorId"] = new SelectList(_context.Processors, "Id", "Name",
                                                     instructionSetExtensionsByProcessor.ProcessorId);
            return View(instructionSetExtensionsByProcessor);
        }

        // POST: InstructionSetExtensionsByProcessor/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,ProcessorId,ExtensionId")]
            InstructionSetExtensionsByProcessor instructionSetExtensionsByProcessor)
        {
            if(id != instructionSetExtensionsByProcessor.Id) return NotFound();

            if(ModelState.IsValid)
            {
                try
                {
                    _context.Update(instructionSetExtensionsByProcessor);
                    await _context.SaveChangesAsync();
                }
                catch(DbUpdateConcurrencyException)
                {
                    if(!InstructionSetExtensionsByProcessorExists(instructionSetExtensionsByProcessor.Id))
                        return NotFound();

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["ExtensionId"] = new SelectList(_context.InstructionSetExtensions, "Id", "Extension",
                                                     instructionSetExtensionsByProcessor.ExtensionId);
            ViewData["ProcessorId"] = new SelectList(_context.Processors, "Id", "Name",
                                                     instructionSetExtensionsByProcessor.ProcessorId);
            return View(instructionSetExtensionsByProcessor);
        }

        // GET: InstructionSetExtensionsByProcessor/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if(id == null) return NotFound();

            InstructionSetExtensionsByProcessor instructionSetExtensionsByProcessor =
                await _context.InstructionSetExtensionsByProcessor.Include(i => i.Extension).Include(i => i.Processor)
                              .FirstOrDefaultAsync(m => m.Id == id);
            if(instructionSetExtensionsByProcessor == null) return NotFound();

            return View(instructionSetExtensionsByProcessor);
        }

        // POST: InstructionSetExtensionsByProcessor/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            InstructionSetExtensionsByProcessor instructionSetExtensionsByProcessor =
                await _context.InstructionSetExtensionsByProcessor.FirstOrDefaultAsync(e => e.Id == id);
            _context.InstructionSetExtensionsByProcessor.Remove(instructionSetExtensionsByProcessor);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        bool InstructionSetExtensionsByProcessorExists(int id)
        {
            return _context.InstructionSetExtensionsByProcessor.Any(e => e.Id == id);
        }

        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> VerifyUnique(int processorId, int extensionId)
        {
            return
                await _context.InstructionSetExtensionsByProcessor.FirstOrDefaultAsync(i =>
                                                                                           i.ProcessorId ==
                                                                                           processorId &&
                                                                                           i.ExtensionId == extensionId)
                    is null
                    ? Json(true)
                    : Json("The selected processor already has the selected extension.");
        }
    }
}