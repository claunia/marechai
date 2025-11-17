using System;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Marechai.App.Presentation.Models;

/// <summary>
///     Data model for a company in the list
/// </summary>
public class CompanyListItem
{
    public int             Id              { get; set; }
    public string          Name            { get; set; } = string.Empty;
    public DateTime?       FoundationDate  { get; set; }
    public SvgImageSource? LogoImageSource { get; set; }

    public string FoundationDateDisplay =>
        FoundationDate.HasValue ? FoundationDate.Value.ToString("MMMM d, yyyy") : string.Empty;
}