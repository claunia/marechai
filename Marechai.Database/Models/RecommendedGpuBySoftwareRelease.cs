namespace Marechai.Database.Models;

public class RecommendedGpuBySoftwareRelease
{
    public         ulong           ReleaseId { get; set; }
    public virtual SoftwareRelease Release   { get; set; }

    public         int GpuId { get; set; }
    public virtual Gpu Gpu   { get; set; }
}