namespace Marechai.Database.Models;

public class SoftwareBySoftwareCompilation
{
    public         ulong               SoftwareCompilationId { get; set; }
    public virtual SoftwareCompilation SoftwareCompilation   { get; set; }

    public         ulong    SoftwareId { get; set; }
    public virtual Software Software   { get; set; }
}
