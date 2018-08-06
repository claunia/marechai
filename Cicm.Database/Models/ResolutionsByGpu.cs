namespace Cicm.Database.Models
{
    public class ResolutionsByGpu
    {
        public int  GpuId        { get; set; }
        public int  ResolutionId { get; set; }
        public long Id           { get; set; }

        public virtual Gpu        Gpu        { get; set; }
        public virtual Resolution Resolution { get; set; }
    }
}