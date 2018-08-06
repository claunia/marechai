namespace Cicm.Database.Models
{
    public class MemoryByMachine
    {
        public int         MachineId { get; set; }
        public MemoryType  Type      { get; set; }
        public MemoryUsage Usage     { get; set; }
        public long?       Size      { get; set; }
        public double?     Speed     { get; set; }
        public long        Id        { get; set; }

        public Machine Machine { get; set; }
    }
}