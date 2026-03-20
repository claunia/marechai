using System;
using Microsoft.UI.Xaml.Media;

namespace Marechai.App.Presentation.Models;

/// <summary>
///     Display item for photo carousel
/// </summary>
public partial class PhotoCarouselDisplayItem : ObservableObject
{
    // Thumbnail constraints
    public const int ThumbnailMaxSize = 256;
    public       Guid PhotoId { get; set; }

    [ObservableProperty]
    private ImageSource? _thumbnailImageSource;
}