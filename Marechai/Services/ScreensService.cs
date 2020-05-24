using System.Collections.Generic;
using System.Linq;
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
    }
}