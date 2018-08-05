using System.Collections.Generic;

namespace Cicm.Database.Models
{
    public class InstructionSetExtensions
    {
        public InstructionSetExtensions()
        {
            InstructionSetExtensionsByProcessor = new HashSet<InstructionSetExtensionsByProcessor>();
        }

        public int    Id        { get; set; }
        public string Extension { get; set; }

        public ICollection<InstructionSetExtensionsByProcessor> InstructionSetExtensionsByProcessor { get; set; }
    }
}