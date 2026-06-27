#nullable enable

using System;
using Microsoft.UI.Xaml.Media;

namespace Marechai.App.Presentation.Models;

public partial class SoftwarePromoArtDisplayItem : ObservableObject
{
    public Guid   PromoArtId { get; set; }
    public string GroupName  { get; set; } = string.Empty;
    public string Caption    { get; set; } = string.Empty;

    public bool HasCaption => !string.IsNullOrWhiteSpace(Caption);

    [ObservableProperty]
    private ImageSource? _thumbnailImageSource;
}
