using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Services
{
    public class InstructionSetsService
    {
        readonly MarechaiContext _context;

        public InstructionSetsService(MarechaiContext context) => _context = context;

        public async Task<List<InstructionSet>> GetAsync() =>
            await _context.InstructionSets.OrderBy(e => e.Name).Select(e => new InstructionSet
            {
                Name = e.Name, Id = e.Id
            }).ToListAsync();

        public async Task<InstructionSet> GetAsync(int id) =>
            await _context.InstructionSets.Where(e => e.Id == id).Select(e => new InstructionSet
            {
                Name = e.Name, Id = e.Id
            }).FirstOrDefaultAsync();

        public async Task UpdateAsync(InstructionSet viewModel)
        {
            InstructionSet model = await _context.InstructionSets.FindAsync(viewModel.Id);

            if(model is null)
                return;

            model.Name = viewModel.Name;

            await _context.SaveChangesAsync();
        }

        public async Task<int> CreateAsync(InstructionSet viewModel)
        {
            var model = new InstructionSet
            {
                Name = viewModel.Name
            };

            await _context.InstructionSets.AddAsync(model);
            await _context.SaveChangesAsync();

            return model.Id;
        }

        public async Task DeleteAsync(int id)
        {
            InstructionSet item = await _context.InstructionSets.FindAsync(id);

            if(item is null)
                return;

            _context.InstructionSets.Remove(item);

            await _context.SaveChangesAsync();
        }

        public bool VerifyUnique(string name) =>
            !_context.InstructionSets.Any(i => string.Equals(i.Name, name,
                                                             StringComparison.InvariantCultureIgnoreCase));
    }
}