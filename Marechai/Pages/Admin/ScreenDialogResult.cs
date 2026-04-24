using System;

namespace Marechai.Pages.Admin;

public sealed class ScreenDialogResult
{
    public double  Diagonal           { get; set; }
    public double? Width              { get; set; }
    public double? Height             { get; set; }
    public long?   EffectiveColors    { get; set; }
    public string? Type               { get; set; }
    public int     NativeResolutionId { get; set; }
}
