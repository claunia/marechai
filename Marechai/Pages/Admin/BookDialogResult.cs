using System;

namespace Marechai.Pages.Admin;

public sealed class BookDialogResult
{
    public string    Title       { get; set; } = null!;
    public string   NativeTitle { get; set; }
    public string   SortTitle   { get; set; }
    public string   Isbn        { get; set; }
    public int?      Edition     { get; set; }
    public int?      Pages       { get; set; }
    public int?      CountryId   { get; set; }
    public DateTime? Published          { get; set; }
    public int       PublishedPrecision  { get; set; }
    public long?     PreviousId  { get; set; }
    public long?     SourceId    { get; set; }
}
