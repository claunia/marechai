namespace Marechai.Database.Models;

public class SoftwareVersionBySoftwareCompilation
{
    public         ulong               SoftwareCompilationId { get; set; }
    public virtual SoftwareCompilation SoftwareCompilation   { get; set; }

    public         ulong           SoftwareVersionId { get; set; }
    public virtual SoftwareVersion SoftwareVersion   { get; set; }
}
