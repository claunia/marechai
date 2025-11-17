namespace Marechai.App.Presentation.Models;

/// <summary>
///     Display item for sound synthesizer information
/// </summary>
public class SoundSynthesizerDisplayItem
{
    public string DisplayName { get; set; } = string.Empty;
    public bool   HasDetails  { get; set; }
    public string DetailsText { get; set; } = string.Empty;
}