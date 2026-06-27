using System;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace Marechai.App.Presentation.Models;

[Bindable]
public partial class MagazineIssueListItem : ObservableObject
{
    public long    Id               { get; set; }
    public string  Caption          { get; set; } = string.Empty;
    public int?    IssueNumber      { get; set; }
    public string? PublishedDisplay { get; set; }
    public Guid?   CoverGuid        { get; set; }

    [ObservableProperty]
    private ImageSource _coverThumbnailSource;
}
