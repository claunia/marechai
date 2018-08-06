namespace Cicm.Database.Models
{
    public class ProcessorsByMachine
    {
        public int    ProcessorId { get; set; }
        public int    MachineId   { get; set; }
        public float? Speed       { get; set; }
        public long   Id          { get; set; }

        public virtual Machine   Machine   { get; set; }
        public virtual Processor Processor { get; set; }
    }
}