using System.Linq;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Marechai.Areas.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class DocumentPeopleController : Controller
    {
        readonly cicmContext _context;

        public DocumentPeopleController(cicmContext context)
        {
            _context = context;
        }

        // GET: DocumentPeople
        public async Task<IActionResult> Index()
        {
            return View(await _context.DocumentPeople.OrderBy(d => d.FullName)
                                      .Select(d => new DocumentPersonViewModel
                                       {
                                           Id       = d.Id,
                                           Name     = d.FullName,
                                           Person   = d.Person.FullName,
                                           PersonId = d.PersonId
                                       }).ToListAsync());
        }

        // GET: DocumentPeople/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if(id == null) return NotFound();

            DocumentPerson documentPerson = await _context.DocumentPeople.FirstOrDefaultAsync(m => m.Id == id);
            if(documentPerson == null) return NotFound();

            return View(documentPerson);
        }

        // GET: DocumentPeople/Create
        public IActionResult Create()
        {
            ViewData["PersonId"] =
                new SelectList(_context.People.OrderBy(c => c.FullName).Select(c => new {c.Id, Name = c.FullName}),
                               "Id", "Name");
            return View();
        }

        // POST: DocumentPeople/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Surname,Alias,DisplayName,PersonId,Id")]
                                                DocumentPerson documentPerson)
        {
            if(!ModelState.IsValid) return View(documentPerson);

            _context.Add(documentPerson);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: DocumentPeople/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if(id == null) return NotFound();

            DocumentPerson documentPerson = await _context.DocumentPeople.FindAsync(id);

            if(documentPerson == null) return NotFound();

            ViewData["PersonId"] =
                new SelectList(_context.People.OrderBy(c => c.FullName).Select(c => new {c.Id, Name = c.FullName}),
                               "Id", "Name", documentPerson.PersonId);

            return View(documentPerson);
        }

        // POST: DocumentPeople/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Name,Surname,Alias,DisplayName,PersonId,Id")]
                                              DocumentPerson documentPerson)
        {
            if(id != documentPerson.Id) return NotFound();

            if(ModelState.IsValid)
            {
                try
                {
                    _context.Update(documentPerson);
                    await _context.SaveChangesAsync();
                }
                catch(DbUpdateConcurrencyException)
                {
                    if(!DocumentPersonExists(documentPerson.Id)) return NotFound();

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["PersonId"] =
                new SelectList(_context.People.OrderBy(c => c.FullName).Select(c => new {c.Id, Name = c.FullName}),
                               "Id", "Name", documentPerson.PersonId);

            return View(documentPerson);
        }

        // GET: DocumentPeople/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if(id == null) return NotFound();

            DocumentPerson documentPerson = await _context.DocumentPeople.FirstOrDefaultAsync(m => m.Id == id);
            if(documentPerson == null) return NotFound();

            return View(documentPerson);
        }

        // POST: DocumentPeople/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            DocumentPerson documentPerson = await _context.DocumentPeople.FindAsync(id);
            _context.DocumentPeople.Remove(documentPerson);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        bool DocumentPersonExists(int id)
        {
            return _context.DocumentPeople.Any(e => e.Id == id);
        }
    }
}