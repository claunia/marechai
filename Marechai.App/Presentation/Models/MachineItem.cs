namespace Marechai.App.Presentation.Models;

/// <summary>
///     Machine item for displaying computers or consoles that use the GPU
/// </summary>
public class MachineItem
{
    public int    Id           { get; set; }
    public string Name         { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public int    Year         { get; set; }

    public string YearDisplay => Year > 0 ? Year.ToString() : "Unknown";
}