using Microsoft.UI.Xaml.Data;

namespace Marechai.App.Presentation.Models;

[Bindable]
public class DocumentListItem
{
    public long   Id    { get; set; }
    public string Title { get; set; } = string.Empty;
    public int?   Year  { get; set; }
}
