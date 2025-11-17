namespace Marechai.App.Presentation.Models;

/// <summary>
///     Data model for a GPU in the list
/// </summary>
public class GpuListItem
{
    public int    Id        { get; set; }
    public string Name      { get; set; } = string.Empty;
    public string Company   { get; set; } = string.Empty;
    public bool   IsSpecial { get; set; }
}