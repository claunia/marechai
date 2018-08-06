namespace Cicm.Database.Models
{
    public class GpusByMachine
    {
        public int  GpuId     { get; set; }
        public int  MachineId { get; set; }
        public long Id        { get; set; }

        public Gpu     Gpu     { get; set; }
        public Machine Machine { get; set; }
    }
}