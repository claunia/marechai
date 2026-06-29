using System;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Marechai.App.Presentation.Models;

/// <summary>
///     Data model for a company in the list
/// </summary>
public class CompanyListItem
{
    public int         Id                 { get; set; }
    public string      Name               { get; set; } = string.Empty;
    public DateTime?   FoundationDate     { get; set; }
    public int?        FoundationPrecision { get; set; }
    public BitmapImage LogoImageSource    { get; set; }

    public string FoundationDateDisplay =>
        DatePrecisionFormatter.Format(FoundationDate, FoundationPrecision) ?? string.Empty;
}
