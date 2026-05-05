using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;

namespace Marechai.App.Models;

public partial class ScreenshotDisplayItem : ObservableObject
{
    public Guid    Id           { get; set; }
    public string Caption      { get; set; }
    public string PlatformName { get; set; }

    [ObservableProperty]
    private ImageSource _thumbnailSource;
}
