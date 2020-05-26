using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Database.Models;

namespace Marechai.Services
{
    public class ScreensService
    {
        readonly MarechaiContext _context;

        public ScreensService(MarechaiContext context) => _context = context;

        public List<Screen> Get() => _context.Screens.AsEnumerable().OrderBy(s => s.Diagonal).
                                              ThenBy(s => s.EffectiveColors).ThenBy(s => s.NativeResolution.ToString()).
                                              ThenBy(s => s.Type).ThenBy(s => s.Size).ToList();

        public async Task<Screen> GetAsync(int id) => await _context.Screens.FindAsync(id);

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