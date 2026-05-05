using System;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace Marechai.App.Presentation.Models;

[Bindable]
public partial class PersonListItem : ObservableObject
{
    public int       Id             { get; set; }
    public string    FullName       { get; set; } = string.Empty;
    public int?      BirthYear      { get; set; }
    public int?      DeathYear      { get; set; }
    public string   CountryOfBirth { get; set; }
    public Guid?     Photo          { get; set; }

    [ObservableProperty]
    private ImageSource _photoThumbnailSource;
}
