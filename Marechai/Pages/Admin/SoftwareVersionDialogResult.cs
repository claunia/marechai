namespace Marechai.Pages.Admin;

public sealed class SoftwareVersionDialogResult
{
    public string  VersionString   { get; set; } = null!;
    public string? PublicVersion   { get; set; }
    public string? Codename        { get; set; }
    public int?    ParentVersionId { get; set; }
    public int?    LicenseId       { get; set; }
}
