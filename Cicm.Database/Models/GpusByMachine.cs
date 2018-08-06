namespace Cicm.Database.Models
{
    public class GpusByMachine
    {
        public int  GpuId     { get; set; }
        public int  MachineId { get; set; }
        public long Id        { get; set; }

        public Gpus     Gpu     { get; set; }
        public Machines Machine { get; set; }
    }
}