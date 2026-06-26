namespace Marechai.Database.Models;

public class SoftwareCompilationBySoftwareCompilation
{
    public         ulong               ParentCompilationId { get; set; }
    public virtual SoftwareCompilation ParentCompilation   { get; set; }

    public         ulong               ChildCompilationId { get; set; }
    public virtual SoftwareCompilation ChildCompilation   { get; set; }
}
