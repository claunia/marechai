namespace Marechai.App.Presentation.Models;

public class SoundSynthListItem
{
    public int     Id        { get; set; }
    public string  Name      { get; set; } = string.Empty;
    public string? Company   { get; set; }
    public bool    IsSpecial { get; set; }
}