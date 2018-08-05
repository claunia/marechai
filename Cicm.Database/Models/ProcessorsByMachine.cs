namespace Cicm.Database.Models
{
    public class ProcessorsByMachine
    {
        public int    Processor { get; set; }
        public int    Machine   { get; set; }
        public float? Speed     { get; set; }
        public long   Id        { get; set; }

        public Machines   MachineNavigation   { get; set; }
        public Processors ProcessorNavigation { get; set; }
    }
}