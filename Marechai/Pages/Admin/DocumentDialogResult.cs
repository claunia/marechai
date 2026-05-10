using System;

namespace Marechai.Pages.Admin;

public sealed class DocumentDialogResult
{
    public string    Title       { get; set; } = null!;
    public string   NativeTitle { get; set; }
    public string   SortTitle   { get; set; }
    public int?      CountryId   { get; set; }
    public DateTime? Published          { get; set; }
    public int       PublishedPrecision  { get; set; }
    public string   InternetArchiveUrl { get; set; }
}
