using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Marechai.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Services
{
    public class ResolutionsByGpuService
    {
        readonly MarechaiContext _context;

        public ResolutionsByGpuService(MarechaiContext context) => _context = context;

        public async Task<List<ResolutionByGpuViewModel>> GetByGpu(int resolutionId) =>
            await _context.ResolutionsByGpu.Where(r => r.ResolutionId == resolutionId).
                           Select(r => new ResolutionByGpuViewModel
                           {
                               Id = r.Id, GpuId = r.GpuId, Resolution = new ResolutionViewModel
                               {
                                   Id     = r.Resolution.Id, Width = r.Resolution.Width, Height = r.Resolution.Height,
                                   Colors = r.Resolution.Colors, Palette = r.Resolution.Palette,
                                   Chars  = r.Resolution.Chars, Grayscale = r.Resolution.Grayscale
                               },
                               ResolutionId = r.ResolutionId
                           }).OrderBy(r => r.Resolution.Width).ThenBy(r => r.Resolution.Height).
                           ThenBy(r => r.Resolution.Chars).ThenBy(r => r.Resolution.Grayscale).
                           ThenBy(r => r.Resolution.Colors).ThenBy(r => r.Resolution.Palette).ToListAsync();

        public async Task DeleteAsync(long id)
        {
            ResolutionsByGpu item = await _context.ResolutionsByGpu.FindAsync(id);

            if(item is null)
                return;

            _context.ResolutionsByGpu.Remove(item);

            await _context.SaveChangesAsync();
        }

        public async Task<long> CreateAsync(int resolutionId, int gpuId)
        {
            var item = new ResolutionsByGpu
            {
                GpuId = gpuId, ResolutionId = resolutionId
            };

            await _context.ResolutionsByGpu.AddAsync(item);
            await _context.SaveChangesAsync();

            return item.Id;
        }
    }
}