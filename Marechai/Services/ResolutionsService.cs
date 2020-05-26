using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Marechai.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Services
{
    public class ResolutionsService
    {
        readonly MarechaiContext _context;

        public ResolutionsService(MarechaiContext context) => _context = context;

        public async Task<List<ResolutionViewModel>> GetAsync()
        {
            List<ResolutionViewModel> list = await _context.Resolutions.Select(r => new ResolutionViewModel
            {
                Id      = r.Id, Width      = r.Width, Height    = r.Height, Colors = r.Colors,
                Palette = r.Palette, Chars = r.Chars, Grayscale = r.Grayscale
            }).ToListAsync();

            return list.OrderBy(r => r.ToString()).ToList();
        }
    }
}