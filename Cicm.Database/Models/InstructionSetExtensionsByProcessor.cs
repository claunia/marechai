namespace Cicm.Database.Models
{
    public class InstructionSetExtensionsByProcessor
    {
        public int Id          { get; set; }
        public int ProcessorId { get; set; }
        public int ExtensionId { get; set; }

        public InstructionSetExtension Extension { get; set; }
        public Processor               Processor { get; set; }
    }
}