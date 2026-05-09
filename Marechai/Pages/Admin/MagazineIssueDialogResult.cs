using System;

namespace Marechai.Pages.Admin;

public sealed class MagazineIssueDialogResult
{
    public long      MagazineId         { get; set; }
    public string    Caption            { get; set; }
    public string    NativeCaption      { get; set; }
    public DateTime? Published          { get; set; }
    public int       PublishedPrecision { get; set; }
    public string    ProductCode        { get; set; }
    public short?    Pages              { get; set; }
    public uint?     IssueNumber        { get; set; }
}
