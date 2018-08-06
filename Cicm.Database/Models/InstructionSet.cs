using System.Collections.Generic;

namespace Cicm.Database.Models
{
    public class InstructionSet
    {
        public InstructionSet()
        {
            Processors = new HashSet<Processor>();
        }

        public int    Id   { get; set; }
        public string Name { get; set; }

        public ICollection<Processor> Processors { get; set; }
    }
}