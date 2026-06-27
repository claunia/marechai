using System.Collections.ObjectModel;

namespace Marechai.App.Presentation.Models;

public sealed class SoftwarePromoArtGroupDisplayItem
{
    public string GroupName { get; set; } = string.Empty;

    public ObservableCollection<SoftwarePromoArtDisplayItem> Items { get; } = [];
}
