using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Marechai.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Services
{
    public class ScreensService
    {
        readonly MarechaiContext _context;

        public ScreensService(MarechaiContext context) => _context = context;

        public List<Screen> Get() => _context.Screens.AsEnumerable().OrderBy(s => s.Diagonal).
                                              ThenBy(s => s.EffectiveColors).ThenBy(s => s.NativeResolution.ToString()).
                                              ThenBy(s => s.Type).ThenBy(s => s.Size).ToList();

        public async Task<ScreenViewModel> GetAsync(int id) =>
            await _context.Screens.Where(s => s.Id == id).Select(s => new ScreenViewModel
            {
                Diagonal = s.Diagonal, EffectiveColors = s.EffectiveColors, Height = s.Height, Id = s.Id,
                NativeResolution = new ResolutionViewModel
                {
                    Chars = s.NativeResolution.Chars, Colors = s.NativeResolution.Colors,
                    Grayscale = s.NativeResolution.Grayscale, Height = s.NativeResolution.Height,
                    Id = s.NativeResolution.Id, Palette = s.NativeResolution.Palette, Width = s.NativeResolution.Width
                },
                NativeResolutionId = s.NativeResolutionId, Type = s.Type, Width = s.Width
            }).FirstOrDefaultAsync();

        public async Task UpdateAsync(ScreenViewModel viewModel)
        {
            Screen model = await _context.Screens.FindAsync(viewModel.Id);

            if(model is null)
                return;

            Resolution nativeResolution = await _context.Resolutions.FindAsync(viewModel.NativeResolutionId);

            if(nativeResolution is null)
                return;

            model.Diagonal           = viewModel.Diagonal;
            model.EffectiveColors    = viewModel.EffectiveColors;
            model.Height             = viewModel.Height;
            model.NativeResolutionId = viewModel.NativeResolutionId;
            model.Type               = viewModel.Type;
            model.Width              = viewModel.Width;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            Screen item = await _context.Screens.FindAsync(id);

            if(item is null)
                return;

            _context.Screens.Remove(item);

            await _context.SaveChangesAsync();
        }
    }
}