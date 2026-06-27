using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;

namespace Marechai.App.Models;

public partial class CoverDisplayItem : ObservableObject
{
    public Guid    Id       { get; set; }
    public string TypeName { get; set; }
    public string Caption  { get; set; }

    [ObservableProperty]
    private ImageSource _thumbnailSource;
}
