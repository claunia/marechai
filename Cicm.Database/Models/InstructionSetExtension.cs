using System.Collections.Generic;

namespace Cicm.Database.Models
{
    public class InstructionSetExtension
    {
        public InstructionSetExtension()
        {
            InstructionSetExtensionsByProcessor = new HashSet<InstructionSetExtensionsByProcessor>();
        }

        public int    Id        { get; set; }
        public string Extension { get; set; }

        public virtual ICollection<InstructionSetExtensionsByProcessor> InstructionSetExtensionsByProcessor
        {
            get;
            set;
        }
    }
}