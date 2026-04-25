using System;

namespace Marechai.Pages.Admin;

public sealed class MagazineDialogResult
{
    public string    Title            { get; set; } = null!;
    public string?   NativeTitle      { get; set; }
    public string?   SortTitle        { get; set; }
    public string?   Issn             { get; set; }
    public int?      CountryId        { get; set; }
    public DateTime? Published        { get; set; }
    public DateTime? FirstPublication { get; set; }
}
