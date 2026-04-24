using System;

namespace Marechai.Pages.Admin;

public sealed class SoundSynthDialogResult
{
    public string    Name       { get; set; } = null!;
    public int?      CompanyId  { get; set; }
    public string?   ModelCode  { get; set; }
    public DateTime? Introduced { get; set; }
    public int?      Voices     { get; set; }
    public double?   Frequency  { get; set; }
    public int?      Depth      { get; set; }
    public int?      SquareWave { get; set; }
    public int?      WhiteNoise { get; set; }
    public int?      Type       { get; set; }
}
