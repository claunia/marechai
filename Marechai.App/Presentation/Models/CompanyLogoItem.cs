using System;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Marechai.App.Presentation.Models;

/// <summary>
///     Data model for a company logo in the carousel
/// </summary>
public class CompanyLogoItem
{
    public Guid            LogoGuid   { get; set; }
    public SvgImageSource? LogoSource { get; set; }
    public int?            Year       { get; set; }
}