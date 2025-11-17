namespace Marechai.App.Presentation.Models;

/// <summary>
///     Display item for GPU information
/// </summary>
public class GpuDisplayItem
{
    public string DisplayName     { get; set; } = string.Empty;
    public string Manufacturer    { get; set; } = string.Empty;
    public bool   HasManufacturer { get; set; }
}