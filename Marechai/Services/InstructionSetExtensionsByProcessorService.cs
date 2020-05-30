using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Marechai.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Services
{
    public class InstructionSetExtensionsByProcessorService
    {
        readonly MarechaiContext _context;

        public InstructionSetExtensionsByProcessorService(MarechaiContext context) => _context = context;

        public async Task<List<InstructionSetExtensionByProcessorViewModel>> GetByProcessor(int processorId) =>
            await _context.InstructionSetExtensionsByProcessor.Where(e => e.ProcessorId == processorId).
                           Select(e => new InstructionSetExtensionByProcessorViewModel
                           {
                               Id          = e.Id, Extension = e.Extension.Extension, Processor = e.Processor.Name,
                               ProcessorId = e.ProcessorId, ExtensionId = e.ExtensionId
                           }).OrderBy(e => e.Extension).ToListAsync();

        public async Task DeleteAsync(int id)
        {
            InstructionSetExtensionsByProcessor item = await _context.InstructionSetExtensionsByProcessor.FindAsync(id);

            if(item is null)
                return;

            _context.InstructionSetExtensionsByProcessor.Remove(item);

            await _context.SaveChangesAsync();
        }

        public async Task<int> CreateAsync(int processorId, int extensionId)
        {
            var item = new InstructionSetExtensionsByProcessor
            {
                ProcessorId = processorId, ExtensionId = extensionId
            };

            await _context.InstructionSetExtensionsByProcessor.AddAsync(item);
            await _context.SaveChangesAsync();

            return item.Id;
        }
    }
}