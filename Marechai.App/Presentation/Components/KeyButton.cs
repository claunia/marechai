using Microsoft.UI.Xaml.Controls;

namespace Marechai.App.Presentation.Components;

/// <summary>
/// A keycap-styled button used by the letter/year browse grids. Its default
/// style (see SharedStyles.xaml) draws the beveled keycap entirely with
/// straight-line Path geometry instead of Border+CornerRadius, since every
/// CornerRadius-based attempt hit a corner-clipping rendering bug on this
/// Uno.Sdk build.
/// </summary>
public sealed partial class KeyButton : Button
{
    public KeyButton()
    {
        DefaultStyleKey = typeof(KeyButton);
    }
}
