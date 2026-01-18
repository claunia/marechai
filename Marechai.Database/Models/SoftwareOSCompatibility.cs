namespace Marechai.Database.Models;

public class SoftwareOSCompatibility
{
    public         ulong           SoftwareVersionId { get; set; }
    public virtual SoftwareVersion SoftwareVersion   { get; set; }
    public         ulong           OSVersionId       { get; set; }
    public virtual SoftwareVersion OSVersion         { get; set; }
}