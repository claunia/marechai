using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Services
{
    public class LicensesService
    {
        readonly MarechaiContext _context;

        public LicensesService(MarechaiContext context) => _context = context;

        public async Task<List<License>> GetAsync() =>
            await _context.Licenses.OrderBy(l => l.Name).Select(l => new License
            {
                FsfApproved = l.FsfApproved, Id   = l.Id, Link = l.Link, Name = l.Name,
                OsiApproved = l.OsiApproved, SPDX = l.SPDX
            }).ToListAsync();

        public async Task<License> GetAsync(int id) =>
            await _context.Licenses.Where(l => l.Id == id).Select(l => new License
            {
                FsfApproved = l.FsfApproved, Id   = l.Id, Link   = l.Link, Name = l.Name,
                OsiApproved = l.OsiApproved, SPDX = l.SPDX, Text = l.Text
            }).FirstOrDefaultAsync();

        public async Task UpdateAsync(License viewModel)
        {
            License model = await _context.Licenses.FindAsync(viewModel.Id);

            if(model is null)
                return;

            model.FsfApproved = viewModel.FsfApproved;
            model.Link        = viewModel.Link;
            model.Name        = viewModel.Name;
            model.OsiApproved = viewModel.OsiApproved;
            model.SPDX        = viewModel.SPDX;
            model.Text        = viewModel.Text;

            await _context.SaveChangesAsync();
        }

        public async Task<int> CreateAsync(License viewModel)
        {
            var model = new License
            {
                FsfApproved = viewModel.FsfApproved, Link = viewModel.Link, Name = viewModel.Name,
                OsiApproved = viewModel.OsiApproved, SPDX = viewModel.SPDX, Text = viewModel.Text
            };

            await _context.Licenses.AddAsync(model);
            await _context.SaveChangesAsync();

            return model.Id;
        }

        public async Task DeleteAsync(int id)
        {
            License item = await _context.Licenses.FindAsync(id);

            if(item is null)
                return;

            _context.Licenses.Remove(item);

            await _context.SaveChangesAsync();
        }
    }
}