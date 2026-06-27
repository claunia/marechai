namespace Marechai.App.Presentation.Models;

/// <summary>
///     Data model for a Tablet in the list
/// </summary>
public class TabletListItem
{
    public int    Id           { get; set; }
    public string Name         { get; set; } = string.Empty;
    public int    Year         { get; set; }
    public string Manufacturer { get; set; } = string.Empty;
}
