namespace Marechai.App.Presentation.Models;

/// <summary>
///     Resolution item for displaying GPU supported resolutions
/// </summary>
public class ResolutionItem
{
    public int    Id        { get; set; }
    public string Name      { get; set; } = string.Empty;
    public int    Width     { get; set; }
    public int    Height    { get; set; }
    public long   Colors    { get; set; }
    public long   Palette   { get; set; }
    public bool   Chars     { get; set; }
    public bool   Grayscale { get; set; }

    public string Resolution => $"{Width}x{Height}";

    public string ResolutionType => Chars ? "Text" : "Pixel";

    public string ResolutionDisplay => Chars ? $"{Width}x{Height} characters" : $"{Width}x{Height}";

    public string ColorDisplay => Grayscale
                                      ? $"{Colors} grays"
                                      : Palette > 0
                                          ? $"{Colors} colors from a palette of {Palette} colors"
                                          : $"{Colors} colors";
}