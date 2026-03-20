using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Marechai.App.Presentation.Models;

/// <summary>
///     Display model for a company logo in the admin management page.
///     Extends ObservableObject because LogoSource is set asynchronously
///     after the item is added to the collection.
/// </summary>
public partial class CompanyLogoDisplayItem : ObservableObject
{
    public int  Id   { get; set; }
    public Guid Guid { get; set; }

    [ObservableProperty]
    private int? _year;

    [ObservableProperty]
    private BitmapImage? _logoSource;
}
