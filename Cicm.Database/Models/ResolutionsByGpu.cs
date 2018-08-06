namespace Cicm.Database.Models
{
    public class ResolutionsByGpu
    {
        public int  GpuId        { get; set; }
        public int  ResolutionId { get; set; }
        public long Id           { get; set; }

        public Gpus        Gpu        { get; set; }
        public Resolutions Resolution { get; set; }
    }
}