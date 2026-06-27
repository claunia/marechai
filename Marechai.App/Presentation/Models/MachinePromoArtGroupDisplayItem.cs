using System.Collections.ObjectModel;

namespace Marechai.App.Presentation.Models;

public sealed class MachinePromoArtGroupDisplayItem
{
    public string GroupName { get; set; } = string.Empty;

    public ObservableCollection<MachinePromoArtDisplayItem> Items { get; } = [];
}
