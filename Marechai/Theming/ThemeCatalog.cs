/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
//
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2003-2026 Natalia Portillo
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using Marechai.Data.Constants;
using MudBlazor;

namespace Marechai.Theming;

/// <summary>
///     Static catalog of selectable UI themes. Each entry is a <see cref="ThemeDefinition" /> carrying a slug, an
///     <see cref="MudTheme" /> instance with palette + typography, and an optional list of web-font CSS URLs to
///     inject when active. To add a new theme: append a constant to <c>Marechai.Data.Constants.ThemeIds</c> (server
///     allow-list), then add a matching <see cref="ThemeDefinition" /> to <see cref="All" />.
/// </summary>
public static class ThemeCatalog
{
    static readonly IReadOnlyList<string> _noFonts = Array.Empty<string>();

    static readonly string[] _robotoStack = ["Roboto", "Helvetica Neue", "Arial", "sans-serif"];

    static readonly string[] _topazStack = ["Topaz", "Courier New", "Consolas", "monospace"];

    static readonly IReadOnlyList<string> _amigaFonts = ["/css/themes/amigaos.css"];

    /// <summary>The historical Marechai dark purple palette. Used as fallback for anonymous + null-preference users.</summary>
    public static readonly ThemeDefinition DefaultDark = new(ThemeIds.DefaultDark,
                                                             "Default (Dark)",
                                                             true,
                                                             new MudTheme
                                                             {
                                                                 PaletteDark = new PaletteDark
                                                                 {
                                                                     Primary          = "#7e57c2",
                                                                     Secondary        = "#42a5f5",
                                                                     AppbarBackground = "#1a1a2e",
                                                                     DrawerBackground = "#16213e",
                                                                     Surface          = "#1e1e2f",
                                                                     Background       = "#121212",
                                                                     TextPrimary      = "#e0e0e0",
                                                                     TextSecondary    = "#a0a0a0",
                                                                     ActionDefault    = "#9e9e9e"
                                                                 },
                                                                 LayoutProperties = new LayoutProperties
                                                                 {
                                                                     DrawerWidthLeft     = "260px",
                                                                     DrawerMiniWidthLeft = "72px"
                                                                 },
                                                                 Typography = new Typography
                                                                 {
                                                                     Default = new DefaultTypography
                                                                     {
                                                                         FontFamily = _robotoStack
                                                                     }
                                                                 }
                                                             },
                                                             _noFonts);

    /// <summary>Stock MudBlazor light palette with the same Roboto typography.</summary>
    public static readonly ThemeDefinition DefaultLight = new(ThemeIds.DefaultLight,
                                                              "Default (Light)",
                                                              false,
                                                              new MudTheme
                                                              {
                                                                  PaletteLight = new PaletteLight(),
                                                                  LayoutProperties = new LayoutProperties
                                                                  {
                                                                      DrawerWidthLeft     = "260px",
                                                                      DrawerMiniWidthLeft = "72px"
                                                                  },
                                                                  Typography = new Typography
                                                                  {
                                                                      Default = new DefaultTypography
                                                                      {
                                                                          FontFamily = _robotoStack
                                                                      }
                                                                  }
                                                              },
                                                              _noFonts);

    /// <summary>
    ///     AmigaOS Workbench 1.x — the iconic blue/white/black/orange 4-colour palette with the Topaz bitmap font.
    ///     Inspired by the original 1985 Amiga Workbench released with the A1000.
    /// </summary>
    public static readonly ThemeDefinition AmigaOs = new(ThemeIds.AmigaOs,
                                                         "AmigaOS Workbench 1.x",
                                                         true,
                                                         new MudTheme
                                                         {
                                                             PaletteDark = new PaletteDark
                                                             {
                                                                 // Workbench 1.x 4-colour palette:
                                                                 //   Color 0 = #0055AA (deep blue, screen / window background)
                                                                 //   Color 1 = #FFFFFF (white,    primary text & title bars)
                                                                 //   Color 2 = #000000 (black,    text on white surfaces)
                                                                 //   Color 3 = #FF8800 (orange,   accent: disk icons, selection)
                                                                 //
                                                                 // Mapping rationale:
                                                                 //   AppBar  = WHITE  (Workbench screen title bar at the top)
                                                                 //   Drawer  = BLUE   (the screen background)
                                                                 //   Surface = BLUE   (the window content area)
                                                                 //   Text    = WHITE  (icon labels on the blue background)
                                                                 //   Primary = ORANGE (used for buttons, badges, highlights —
                                                                 //                     i.e. the colour Workbench reserved for
                                                                 //                     disk icons and selection indicators)
                                                                 Primary                  = "#FF8800",
                                                                 PrimaryContrastText      = "#000000",
                                                                 Secondary                = "#FFFFFF",
                                                                 SecondaryContrastText    = "#000000",
                                                                 Tertiary                 = "#000000",
                                                                 TertiaryContrastText     = "#FFFFFF",
                                                                 AppbarBackground         = "#FFFFFF",
                                                                 AppbarText               = "#000000",
                                                                 DrawerBackground         = "#0055AA",
                                                                 DrawerText               = "#FFFFFF",
                                                                 DrawerIcon               = "#FFFFFF",
                                                                 Surface                  = "#0055AA",
                                                                 Background               = "#0055AA",
                                                                 BackgroundGray           = "#003C7A",
                                                                 TextPrimary              = "#FFFFFF",
                                                                 TextSecondary            = "#FFCC88",
                                                                 TextDisabled             = "#88AACC",
                                                                 ActionDefault            = "#FFFFFF",
                                                                 ActionDisabled           = "#88AACC",
                                                                 ActionDisabledBackground = "#003C7A",
                                                                 LinesDefault             = "#FFFFFF",
                                                                 LinesInputs              = "#FFFFFF",
                                                                 TableLines               = "#FFFFFF",
                                                                 TableStriped             = "#003C7A",
                                                                 TableHover               = "#1A6BBF",
                                                                 Divider                  = "#FFFFFF",
                                                                 DividerLight             = "#88AACC",
                                                                 // Status colours stay close to the period palette:
                                                                 //   Info    = white (period UI used white for neutral notices)
                                                                 //   Success = green (Workbench had no green; pick a CGA-era tint)
                                                                 //   Warning = orange (the accent colour)
                                                                 //   Error   = red    (period error red)
                                                                 Info                     = "#FFFFFF",
                                                                 Success                  = "#88EE88",
                                                                 Warning                  = "#FF8800",
                                                                 Error                    = "#FF6666",
                                                                 Dark                     = "#000000"
                                                             },
                                                             LayoutProperties = new LayoutProperties
                                                             {
                                                                 DrawerWidthLeft     = "260px",
                                                                 DrawerMiniWidthLeft = "72px"
                                                             },
                                                             Typography = new Typography
                                                             {
                                                                 Default = new DefaultTypography
                                                                 {
                                                                     FontFamily   = _topazStack,
                                                                     FontSize     = "0.875rem",
                                                                     FontWeight   = "400",
                                                                     LineHeight   = "1.5",
                                                                     LetterSpacing = "0"
                                                                 },
                                                                 H1 = new H1Typography
                                                                 {
                                                                     FontFamily = _topazStack,
                                                                     FontSize   = "2rem",
                                                                     FontWeight = "700"
                                                                 },
                                                                 H2 = new H2Typography
                                                                 {
                                                                     FontFamily = _topazStack,
                                                                     FontSize   = "1.75rem",
                                                                     FontWeight = "700"
                                                                 },
                                                                 H3 = new H3Typography
                                                                 {
                                                                     FontFamily = _topazStack,
                                                                     FontSize   = "1.5rem",
                                                                     FontWeight = "700"
                                                                 },
                                                                 H4 = new H4Typography
                                                                 {
                                                                     FontFamily = _topazStack,
                                                                     FontSize   = "1.25rem",
                                                                     FontWeight = "700"
                                                                 },
                                                                 H5 = new H5Typography
                                                                 {
                                                                     FontFamily = _topazStack,
                                                                     FontSize   = "1.125rem",
                                                                     FontWeight = "700"
                                                                 },
                                                                 H6 = new H6Typography
                                                                 {
                                                                     FontFamily = _topazStack,
                                                                     FontSize   = "1rem",
                                                                     FontWeight = "700"
                                                                 },
                                                                 Subtitle1 = new Subtitle1Typography
                                                                 {
                                                                     FontFamily = _topazStack,
                                                                     FontWeight = "700"
                                                                 },
                                                                 Subtitle2 = new Subtitle2Typography
                                                                 {
                                                                     FontFamily = _topazStack,
                                                                     FontWeight = "700"
                                                                 },
                                                                 Body1 = new Body1Typography
                                                                 {
                                                                     FontFamily = _topazStack
                                                                 },
                                                                 Body2 = new Body2Typography
                                                                 {
                                                                     FontFamily = _topazStack
                                                                 },
                                                                 Button = new ButtonTypography
                                                                 {
                                                                     FontFamily   = _topazStack,
                                                                     FontWeight    = "700",
                                                                     TextTransform = "none"
                                                                 },
                                                                 Caption = new CaptionTypography
                                                                 {
                                                                     FontFamily = _topazStack
                                                                 },
                                                                 Overline = new OverlineTypography
                                                                 {
                                                                     FontFamily    = _topazStack,
                                                                     FontWeight    = "700",
                                                                     TextTransform = "uppercase"
                                                                 }
                                                             }
                                                         },
                                                         _amigaFonts);

    /// <summary>All themes available to users in the Appearance picker. Order matters — it's the display order.</summary>
    public static readonly IReadOnlyList<ThemeDefinition> All = new[] { DefaultDark, DefaultLight, AmigaOs };

    /// <summary>The default theme used when the user has no preference set.</summary>
    public static ThemeDefinition Default => DefaultDark;

    /// <summary>
    ///     Resolve a slug to a known <see cref="ThemeDefinition" />, or <see langword="null" /> if the slug is unknown
    ///     or empty/whitespace. Lookup is case-insensitive.
    /// </summary>
    public static ThemeDefinition FindById(string id)
    {
        if(string.IsNullOrWhiteSpace(id)) return null;

        return All.FirstOrDefault(t => string.Equals(t.Id, id, StringComparison.OrdinalIgnoreCase));
    }
}
