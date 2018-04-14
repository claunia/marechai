using System.Collections.Generic;
using System.Linq;
using cicm_web.Models;
using Microsoft.AspNetCore.Mvc;
using Computer = Cicm.Database.Schemas.Computer;

namespace cicm_web.Controllers
{
    public class ComputerController : Controller
    {
        public IActionResult Index()
        {
            Program.Database.Operations.GetComputers(out List<Computer> computers);

            ViewBag.ItemCount = computers.Count;

            ViewBag.MinYear = computers.Where(t => t.Year > 1000).Min(t => t.Year);
            ViewBag.MaxYear = computers.Where(t => t.Year > 1000).Max(t => t.Year);

            return View();
        }

        public IActionResult ByLetter(char id)
        {
            // ToUpper()
            if(id >= 'a' && id <= 'z') id -= (char)32;
            // Check if not letter
            if(id < 'A' || id > 'Z') id = '\0';

            ViewBag.Letter = id;

            ComputerMini[] computers =
                id == '\0' ? ComputerMini.GetAllItems() : ComputerMini.GetItemsStartingWithLetter(id);

            return View(computers);
        }
    }
}