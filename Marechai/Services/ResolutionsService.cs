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

        public async Task<List<ResolutionViewModel>> GetAsync() =>
            await _context.Resolutions.Select(r => new ResolutionViewModel
                           {
                               Id      = r.Id, Width      = r.Width, Height    = r.Height, Colors = r.Colors,
                               Palette = r.Palette, Chars = r.Chars, Grayscale = r.Grayscale
                           }).OrderBy(r => r.Width).ThenBy(r => r.Height).ThenBy(r => r.Chars).ThenBy(r => r.Grayscale).
                           ThenBy(r => r.Colors).ThenBy(r => r.Palette).ToListAsync();

        public async Task<ResolutionViewModel> GetAsync(int id) =>
            await _context.Resolutions.Where(r => r.Id == id).Select(r => new ResolutionViewModel
            {
                Id      = r.Id, Width      = r.Width, Height    = r.Height, Colors = r.Colors,
                Palette = r.Palette, Chars = r.Chars, Grayscale = r.Grayscale
            }).FirstOrDefaultAsync();

        public async Task UpdateAsync(ResolutionViewModel viewModel)
        {
            Resolution model = await _context.Resolutions.FindAsync(viewModel.Id);

            if(model is null)
                return;

            model.Chars     = viewModel.Chars;
            model.Colors    = viewModel.Colors;
            model.Grayscale = viewModel.Grayscale;
            model.Height    = viewModel.Height;
            model.Palette   = viewModel.Palette;
            model.Width     = viewModel.Width;

            await _context.SaveChangesAsync();
        }

        public async Task<int> CreateAsync(ResolutionViewModel viewModel)
        {
            var model = new Resolution
            {
                Chars  = viewModel.Chars, Colors   = viewModel.Colors, Grayscale = viewModel.Grayscale,
                Height = viewModel.Height, Palette = viewModel.Palette, Width    = viewModel.Width
            };

            await _context.Resolutions.AddAsync(model);
            await _context.SaveChangesAsync();

            return model.Id;
        }

        public async Task DeleteAsync(int id)
        {
            Resolution item = await _context.Resolutions.FindAsync(id);

            if(item is null)
                return;

            _context.Resolutions.Remove(item);

            await _context.SaveChangesAsync();
        }
    }
}