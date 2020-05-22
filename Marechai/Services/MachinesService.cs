using System.Threading.Tasks;
using Marechai.Database.Models;
using Microsoft.Extensions.Localization;

namespace Marechai.Services
{
    public class MachinesService
    {
        readonly MarechaiContext                   _context;
        readonly IStringLocalizer<MachinesService> _l;

        public MachinesService(MarechaiContext context, IStringLocalizer<MachinesService> localizer)
        {
            _context = context;
            _l       = localizer;
        }

        public async Task<Machine> GetMachine(int id) => await _context.Machines.FindAsync(id);
    }
}