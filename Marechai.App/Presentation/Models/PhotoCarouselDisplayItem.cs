using System;
using Microsoft.UI.Xaml.Media;

namespace Marechai.App.Presentation.Models;

/// <summary>
///     Display item for photo carousel
/// </summary>
public class PhotoCarouselDisplayItem
{
    // Thumbnail constraints
    public const int          ThumbnailMaxSize = 256;
    public       Guid         PhotoId              { get; set; }
    public       ImageSource? ThumbnailImageSource { get; set; }
}