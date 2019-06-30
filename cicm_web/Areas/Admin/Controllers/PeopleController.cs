using System.Linq;
using System.Threading.Tasks;
using Cicm.Database.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace cicm_web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class PeopleController : Controller
    {
        readonly cicmContext _context;

        public PeopleController(cicmContext context)
        {
            _context = context;
        }

        // GET: People
        public async Task<IActionResult> Index()
        {
            IIncludableQueryable<Person, Iso31661Numeric> cicmContext = _context.People.Include(p => p.CountryOfBirth);
            return View(await cicmContext.OrderBy(p => p.FullName).ToListAsync());
        }

        // GET: People/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if(id == null) return NotFound();

            Person person = await _context.People.Include(p => p.CountryOfBirth).FirstOrDefaultAsync(m => m.Id == id);
            if(person == null) return NotFound();

            return View(person);
        }

        // GET: People/Create
        public IActionResult Create()
        {
            ViewData["CountryOfBirthId"] = new SelectList(_context.Iso31661Numeric, "Id", "Name");
            return View();
        }

        // POST: People/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind(
                "Name,Surname,BirthDate,DeathDate,Webpage,Twitter,Facebook,Photo,CountryOfBirthId,Id,Alias,DisplayName")]
            Person person)
        {
            if(ModelState.IsValid)
            {
                _context.Add(person);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CountryOfBirthId"] =
                new SelectList(_context.Iso31661Numeric, "Id", "Name", person.CountryOfBirthId);
            return View(person);
        }

        // GET: People/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if(id == null) return NotFound();

            Person person = await _context.People.FindAsync(id);
            if(person == null) return NotFound();

            ViewData["CountryOfBirthId"] =
                new SelectList(_context.Iso31661Numeric, "Id", "Name", person.CountryOfBirthId);
            return View(person);
        }

        // POST: People/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id, [Bind(
                "Name,Surname,BirthDate,DeathDate,Webpage,Twitter,Facebook,Photo,CountryOfBirthId,Id,Alias,DisplayName")]
            Person person)
        {
            if(id != person.Id) return NotFound();

            if(ModelState.IsValid)
            {
                try
                {
                    _context.Update(person);
                    await _context.SaveChangesAsync();
                }
                catch(DbUpdateConcurrencyException)
                {
                    if(!PersonExists(person.Id)) return NotFound();

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["CountryOfBirthId"] =
                new SelectList(_context.Iso31661Numeric, "Id", "Name", person.CountryOfBirthId);
            return View(person);
        }

        // GET: People/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if(id == null) return NotFound();

            Person person = await _context.People.Include(p => p.CountryOfBirth).FirstOrDefaultAsync(m => m.Id == id);
            if(person == null) return NotFound();

            return View(person);
        }

        // POST: People/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Person person = await _context.People.FindAsync(id);
            _context.People.Remove(person);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        bool PersonExists(int id)
        {
            return _context.People.Any(e => e.Id == id);
        }

        [AcceptVerbs("Get", "Post")]
        public IActionResult VerifyTwitter(string twitter) =>
            twitter?.Length > 0 && twitter[0] == '@' ? Json(true) : Json("Invalid twitter handle.");
    }
}