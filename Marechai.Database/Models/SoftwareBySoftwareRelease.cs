namespace Marechai.Database.Models;

public class SoftwareBySoftwareRelease
{
    public         ulong           ReleaseId { get; set; }
    public virtual SoftwareRelease Release   { get; set; }

    public         ulong    SoftwareId { get; set; }
    public virtual Software Software   { get; set; }
}
