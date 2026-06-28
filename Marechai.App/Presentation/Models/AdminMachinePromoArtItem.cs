#nullable enable

using System;
using Microsoft.UI.Xaml.Media;

namespace Marechai.App.Presentation.Models;

public partial class AdminMachinePromoArtItem : ObservableObject
{
    public Guid Id { get; set; }

    [ObservableProperty]
    private string _groupName = string.Empty;

    [ObservableProperty]
    private string _caption = string.Empty;

    [ObservableProperty]
    private ImageSource? _thumbnailImageSource;
}
