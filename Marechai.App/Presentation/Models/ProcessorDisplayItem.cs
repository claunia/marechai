namespace Marechai.App.Presentation.Models;

/// <summary>
///     Display item for processor information
/// </summary>
public class ProcessorDisplayItem
{
    public int    Id           { get; set; }
    public string DisplayName  { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public bool   HasDetails   { get; set; }
    public string DetailsText  { get; set; } = string.Empty;
}
