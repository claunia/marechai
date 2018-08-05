namespace Cicm.Database.Models
{
    public class GpusByMachine
    {
        public int  Gpu     { get; set; }
        public int  Machine { get; set; }
        public long Id      { get; set; }

        public Gpus     GpuNavigation     { get; set; }
        public Machines MachineNavigation { get; set; }
    }
}