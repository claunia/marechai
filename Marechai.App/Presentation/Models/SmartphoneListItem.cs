namespace Marechai.App.Presentation.Models;

/// <summary>
///     Data model for a smartphone in the list
/// </summary>
public class SmartphoneListItem
{
    public int    Id           { get; set; }
    public string Name         { get; set; } = string.Empty;
    public int    Year         { get; set; }
    public string Manufacturer { get; set; } = string.Empty;
}
