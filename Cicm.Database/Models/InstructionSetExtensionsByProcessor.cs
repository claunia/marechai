namespace Cicm.Database.Models
{
    public class InstructionSetExtensionsByProcessor
    {
        public int Id          { get; set; }
        public int ProcessorId { get; set; }
        public int ExtensionId { get; set; }

        public virtual InstructionSetExtension Extension { get; set; }
        public virtual Processor               Processor { get; set; }
    }
}