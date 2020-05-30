using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Marechai.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Services
{
    public class ResolutionsByScreenService
    {
        readonly MarechaiContext _context;

        public ResolutionsByScreenService(MarechaiContext context) => _context = context;

        public async Task<List<ResolutionByScreenViewModel>> GetByScreen(int resolutionId) =>
            (await _context.ResolutionsByScreen.Where(r => r.ResolutionId == resolutionId).
                            Select(r => new ResolutionByScreenViewModel
                            {
                                Id = r.Id, ScreenId = r.ScreenId, Resolution = new ResolutionViewModel
                                {
                                    Id     = r.Resolution.Id, Width = r.Resolution.Width, Height = r.Resolution.Height,
                                    Colors = r.Resolution.Colors, Palette = r.Resolution.Palette,
                                    Chars  = r.Resolution.Chars, Grayscale = r.Resolution.Grayscale
                                },
                                ResolutionId = r.ResolutionId
                            }).ToListAsync()).OrderBy(r => r.Resolution.Width).ThenBy(r => r.Resolution.Height).
                                              ThenBy(r => r.Resolution.Chars).ThenBy(r => r.Resolution.Grayscale).
                                              ThenBy(r => r.Resolution.Colors).ThenBy(r => r.Resolution.Palette).
                                              ToList();

        public async Task DeleteAsync(long id)
        {
            ResolutionsByScreen item = await _context.ResolutionsByScreen.FindAsync(id);

            if(item is null)
                return;

            _context.ResolutionsByScreen.Remove(item);

            await _context.SaveChangesAsync();
        }

        public async Task<long> CreateAsync(int resolutionId, int screenId)
        {
            var item = new ResolutionsByScreen
            {
                ScreenId = screenId, ResolutionId = resolutionId
            };

            await _context.ResolutionsByScreen.AddAsync(item);
            await _context.SaveChangesAsync();

            return item.Id;
        }
    }
}