/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : NewsController.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     News admin controller
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2003-2018 Natalia Portillo
*******************************************************************************/

using System.Linq;
using System.Threading.Tasks;
using Cicm.Database.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cicm_web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class NewsController : Controller
    {
        readonly cicmContext _context;

        public NewsController(cicmContext context)
        {
            _context = context;
        }

        // GET: Admin/News
        public async Task<IActionResult> Index() =>
            View(await _context.News.OrderByDescending(n => n.Date).ToListAsync());

        // GET: Admin/News/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if(id == null) return NotFound();

            News news = await _context.News.FirstOrDefaultAsync(m => m.Id == id);
            if(news == null) return NotFound();

            return View(news);
        }

        // POST: Admin/News/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            News news = await _context.News.FindAsync(id);
            _context.News.Remove(news);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        bool NewsExists(int id)
        {
            return _context.News.Any(e => e.Id == id);
        }
    }
}