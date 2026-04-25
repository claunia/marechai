namespace Marechai.Database.Models;

public class SoftwareVersionBySoftwareRelease
{
    public         ulong           ReleaseId         { get; set; }
    public virtual SoftwareRelease Release           { get; set; }

    public         ulong           SoftwareVersionId { get; set; }
    public virtual SoftwareVersion SoftwareVersion   { get; set; }
}
