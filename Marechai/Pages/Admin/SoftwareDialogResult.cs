using Marechai.Data;

namespace Marechai.Pages.Admin;

public sealed class SoftwareDialogResult
{
    public string       Name            { get; set; } = null!;
    public int?         FamilyId        { get; set; }
    public int?         PredecessorId   { get; set; }
    public int?         BaseSoftwareId  { get; set; }
    public SoftwareKind Kind            { get; set; }
}
