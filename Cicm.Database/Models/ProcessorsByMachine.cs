namespace Cicm.Database.Models
{
    public class ProcessorsByMachine
    {
        public int    ProcessorId { get; set; }
        public int    MachineId   { get; set; }
        public float? Speed       { get; set; }
        public long   Id          { get; set; }

        public Machines   Machine   { get; set; }
        public Processors Processor { get; set; }
    }
}