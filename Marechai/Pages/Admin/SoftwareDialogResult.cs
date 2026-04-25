namespace Marechai.Pages.Admin;

public sealed class SoftwareDialogResult
{
    public string Name              { get; set; } = null!;
    public int?   FamilyId          { get; set; }
    public bool   IsOperatingSystem { get; set; }
    public bool   IsGame            { get; set; }
}
