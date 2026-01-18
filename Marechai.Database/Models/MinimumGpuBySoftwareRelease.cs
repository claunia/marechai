namespace Marechai.Database.Models;

public class MinimumGpuBySoftwareRelease
{
    public         ulong           ReleaseId { get; set; }
    public virtual SoftwareRelease Release   { get; set; }

    public         int GpuId { get; set; }
    public virtual Gpu Gpu   { get; set; }
}