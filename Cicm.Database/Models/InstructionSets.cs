using System.Collections.Generic;

namespace Cicm.Database.Models
{
    public class InstructionSets
    {
        public InstructionSets()
        {
            Processors = new HashSet<Processors>();
        }

        public int    Id             { get; set; }
        public string InstructionSet { get; set; }

        public ICollection<Processors> Processors { get; set; }
    }
}