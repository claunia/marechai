using System;

namespace Marechai.Pages.Admin;

public sealed class SoftwareFamilyDialogResult
{
    public string    Name       { get; set; } = null!;
    public int?      ParentId   { get; set; }
    public DateTime? Introduced { get; set; }
}
