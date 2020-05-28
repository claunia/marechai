using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Services
{
    public class InstructionSetExtensionsService
    {
        readonly MarechaiContext _context;

        public InstructionSetExtensionsService(MarechaiContext context) => _context = context;

        public async Task<List<InstructionSetExtension>> GetAsync() =>
            await _context.InstructionSetExtensions.OrderBy(e => e.Extension).Select(e => new InstructionSetExtension
            {
                Extension = e.Extension, Id = e.Id
            }).ToListAsync();

        public async Task<InstructionSetExtension> GetAsync(int id) =>
            await _context.InstructionSetExtensions.Where(e => e.Id == id).Select(e => new InstructionSetExtension
            {
                Extension = e.Extension, Id = e.Id
            }).FirstOrDefaultAsync();

        public async Task UpdateAsync(InstructionSetExtension viewModel)
        {
            InstructionSetExtension model = await _context.InstructionSetExtensions.FindAsync(viewModel.Id);

            if(model is null)
                return;

            model.Extension = viewModel.Extension;

            await _context.SaveChangesAsync();
        }

        public async Task<int> CreateAsync(InstructionSetExtension viewModel)
        {
            var model = new InstructionSetExtension
            {
                Extension = viewModel.Extension
            };

            await _context.InstructionSetExtensions.AddAsync(model);
            await _context.SaveChangesAsync();

            return model.Id;
        }

        public async Task DeleteAsync(int id)
        {
            InstructionSetExtension item = await _context.InstructionSetExtensions.FindAsync(id);

            if(item is null)
                return;

            _context.InstructionSetExtensions.Remove(item);

            await _context.SaveChangesAsync();
        }

        public bool VerifyUnique(string extension) =>
            !_context.InstructionSetExtensions.Any(i => string.Equals(i.Extension, extension,
                                                                      StringComparison.InvariantCultureIgnoreCase));
    }
}