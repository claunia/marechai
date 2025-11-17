namespace Marechai.App.Presentation.Models;

/// <summary>
///     Data model for a Processor in the list
/// </summary>
public class ProcessorListItem
{
    public int    Id      { get; set; }
    public string Name    { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
}