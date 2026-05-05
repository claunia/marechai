using System;

namespace Marechai.Pages.Admin;

public sealed class MachineDialogResult
{
    public string    Name       { get; set; } = null!;
    public string   Model      { get; set; }
    public int?      CompanyId  { get; set; }
    public int       Type       { get; set; }
    public bool      Prototype  { get; set; }
    public DateTime? Introduced          { get; set; }
    public int       IntroducedPrecision { get; set; }
    public int?      FamilyId            { get; set; }
}
