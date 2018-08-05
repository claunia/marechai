namespace Cicm.Database.Models
{
    public class ResolutionsByGpu
    {
        public int  Gpu        { get; set; }
        public int  Resolution { get; set; }
        public long Id         { get; set; }

        public Gpus        GpuNavigation        { get; set; }
        public Resolutions ResolutionNavigation { get; set; }
    }
}