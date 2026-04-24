namespace Marechai.Pages.Admin;

public sealed class ResolutionDialogResult
{
    public int   Width     { get; set; }
    public int   Height    { get; set; }
    public long? Colors    { get; set; }
    public long? Palette   { get; set; }
    public bool  Chars     { get; set; }
    public bool  Grayscale { get; set; }
}
