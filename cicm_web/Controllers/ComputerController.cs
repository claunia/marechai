using System.Collections.Generic;
using System.Linq;
using Cicm.Database.Schemas;
using Microsoft.AspNetCore.Mvc;

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
    }
}