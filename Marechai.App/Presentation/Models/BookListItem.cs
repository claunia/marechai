using System;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace Marechai.App.Presentation.Models;

[Bindable]
public partial class BookListItem : ObservableObject
{
    public long    Id        { get; set; }
    public string  Title     { get; set; } = string.Empty;
    public int?    Year      { get; set; }
    public string Authors   { get; set; }
    public Guid?   CoverGuid { get; set; }

    [ObservableProperty]
    private ImageSource _coverThumbnailSource;
}
