namespace Cicm.Database.Models
{
    public class InstructionSetExtensionsByProcessor
    {
        public int Id          { get; set; }
        public int ProcessorId { get; set; }
        public int ExtensionId { get; set; }

        public InstructionSetExtensions Extension { get; set; }
        public Processors               Processor { get; set; }
    }
}