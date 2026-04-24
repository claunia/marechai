using System;

namespace Marechai.Pages.Admin;

public sealed class GpuDialogResult
{
    public string    Name        { get; set; } = null!;
    public int?      CompanyId   { get; set; }
    public string?   ModelCode   { get; set; }
    public DateTime? Introduced  { get; set; }
    public string?   Package     { get; set; }
    public string?   Process     { get; set; }
    public float?    ProcessNm   { get; set; }
    public float?    DieSize     { get; set; }
    public long?     Transistors { get; set; }
}
