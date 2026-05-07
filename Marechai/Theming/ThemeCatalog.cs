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

    static readonly string[] _ibmVgaStack = ["IBM VGA", "Courier New", "Consolas", "monospace"];

    static readonly IReadOnlyList<string> _dosFonts = ["/css/themes/dos.css"];

    static readonly string[] _chiKareGoStack = ["ChiKareGo", "Charcoal", "Geneva", "Helvetica Neue", "Arial", "sans-serif"];

    static readonly IReadOnlyList<string> _macOs9Fonts = ["/css/themes/macos9.css"];

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

    /// <summary>
    ///     DOS — the iconic Borland Turbo Vision text-mode UI: bright CGA blue desktop, light gray pop-up dialogs,
    ///     signature green action buttons, cyan input fields, red menu hotkeys, and the IBM VGA 9x16 raster font.
    ///     Inspired by Turbo Pascal 7, Turbo C++ 3, and the original Turbo Vision 2.0 (1994).
    /// </summary>
    public static readonly ThemeDefinition Dos = new(ThemeIds.Dos,
                                                     "DOS (Turbo Vision)",
                                                     true,
                                                     new MudTheme
                                                     {
                                                         PaletteDark = new PaletteDark
                                                         {
                                                             // Standard CGA / VGA text-mode 16-colour palette:
                                                             //   Blue    = #0000AA  (desktop background)
                                                             //   Gray    = #AAAAAA  (dialog & menu-bar surface)
                                                             //   Green   = #00AA00  (Turbo Vision OK / Save buttons only)
                                                             //   Cyan    = #00AAAA  (selected items, input fields, accents — the dominant accent)
                                                             //   Red     = #AA0000  (menu hotkeys, errors)
                                                             //   Yellow  = #AAAA00  (warnings — kept dimmer than CGA bright)
                                                             //   Black   = #000000  (text on gray)
                                                             //   White   = #FFFFFF  (text on blue / on cyan)
                                                             //
                                                             // Mapping rationale:
                                                             //   AppBar    = GRAY  (the top menu bar)
                                                             //   Drawer    = BLUE  (the desktop / nav surface)
                                                             //   Surface   = GRAY  (pop-up dialogs / cards)
                                                             //   Background= BLUE  (the desktop body)
                                                             //   Primary   = CYAN  (input fields, selection, focused state — by far the
                                                             //                      most-used interactive colour in Turbo Vision; mapping
                                                             //                      this slot to green made tabs/switches/progress bars
                                                             //                      look out of place since green was only ever on
                                                             //                      OK / Cancel buttons in the actual UI)
                                                             //   Secondary = YELLOW(highlighted menu items / submenu selection)
                                                             //   Tertiary  = RED   (hotkeys / shortcut letters)
                                                             //   Success   = GREEN (preserves the iconic "OK button is green" only
                                                             //                      where explicit Color.Success is requested)
                                                             Primary                  = "#00AAAA",
                                                             PrimaryContrastText      = "#000000",
                                                             Secondary                = "#FFFF55",
                                                             SecondaryContrastText    = "#000000",
                                                             Tertiary                 = "#AA0000",
                                                             TertiaryContrastText     = "#FFFFFF",
                                                             AppbarBackground         = "#AAAAAA",
                                                             AppbarText               = "#000000",
                                                             DrawerBackground         = "#0000AA",
                                                             DrawerText               = "#FFFFFF",
                                                             DrawerIcon               = "#FFFFFF",
                                                             Surface                  = "#AAAAAA",
                                                             Background               = "#0000AA",
                                                             BackgroundGray           = "#000088",
                                                             TextPrimary              = "#000000",
                                                             TextSecondary            = "#0000AA",
                                                             TextDisabled             = "#555555",
                                                             ActionDefault            = "#000000",
                                                             ActionDisabled           = "#555555",
                                                             ActionDisabledBackground = "#888888",
                                                             LinesDefault             = "#000000",
                                                             LinesInputs              = "#000000",
                                                             TableLines               = "#555555",
                                                             TableStriped             = "#888888",
                                                             TableHover               = "#00AAAA",
                                                             Divider                  = "#555555",
                                                             DividerLight             = "#888888",
                                                             Info                     = "#00AAAA",
                                                             Success                  = "#00AA00",
                                                             Warning                  = "#AA5500",
                                                             Error                    = "#AA0000",
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
                                                                 FontFamily    = _ibmVgaStack,
                                                                 FontSize      = "0.875rem",
                                                                 FontWeight    = "400",
                                                                 LineHeight    = "1.4",
                                                                 LetterSpacing = "0"
                                                             },
                                                             H1 = new H1Typography
                                                             {
                                                                 FontFamily = _ibmVgaStack,
                                                                 FontSize   = "2rem",
                                                                 FontWeight = "700"
                                                             },
                                                             H2 = new H2Typography
                                                             {
                                                                 FontFamily = _ibmVgaStack,
                                                                 FontSize   = "1.75rem",
                                                                 FontWeight = "700"
                                                             },
                                                             H3 = new H3Typography
                                                             {
                                                                 FontFamily = _ibmVgaStack,
                                                                 FontSize   = "1.5rem",
                                                                 FontWeight = "700"
                                                             },
                                                             H4 = new H4Typography
                                                             {
                                                                 FontFamily = _ibmVgaStack,
                                                                 FontSize   = "1.25rem",
                                                                 FontWeight = "700"
                                                             },
                                                             H5 = new H5Typography
                                                             {
                                                                 FontFamily = _ibmVgaStack,
                                                                 FontSize   = "1.125rem",
                                                                 FontWeight = "700"
                                                             },
                                                             H6 = new H6Typography
                                                             {
                                                                 FontFamily = _ibmVgaStack,
                                                                 FontSize   = "1rem",
                                                                 FontWeight = "700"
                                                             },
                                                             Subtitle1 = new Subtitle1Typography
                                                             {
                                                                 FontFamily = _ibmVgaStack,
                                                                 FontWeight = "700"
                                                             },
                                                             Subtitle2 = new Subtitle2Typography
                                                             {
                                                                 FontFamily = _ibmVgaStack,
                                                                 FontWeight = "700"
                                                             },
                                                             Body1 = new Body1Typography
                                                             {
                                                                 FontFamily = _ibmVgaStack
                                                             },
                                                             Body2 = new Body2Typography
                                                             {
                                                                 FontFamily = _ibmVgaStack
                                                             },
                                                             Button = new ButtonTypography
                                                             {
                                                                 FontFamily    = _ibmVgaStack,
                                                                 FontWeight    = "700",
                                                                 TextTransform = "none"
                                                             },
                                                             Caption = new CaptionTypography
                                                             {
                                                                 FontFamily = _ibmVgaStack
                                                             },
                                                             Overline = new OverlineTypography
                                                             {
                                                                 FontFamily    = _ibmVgaStack,
                                                                 FontWeight    = "700",
                                                                 TextTransform = "uppercase"
                                                             }
                                                         }
                                                     },
                                                     _dosFonts);

    /// <summary>
    ///     Mac OS 9 — the Platinum theme: lavender purple-gray desktop, white pinstriped window chrome, royal-blue
    ///     accent for default actions and selection, and the Virtue Charcoal-style display font for headings.
    ///     Inspired by Mac OS 8.5 — 9.2.2 (1998 — 2001).
    /// </summary>
    public static readonly ThemeDefinition MacOs9 = new(ThemeIds.MacOs9,
                                                        "Mac OS 9 (Platinum)",
                                                        false,
                                                        new MudTheme
                                                        {
                                                            // Mac OS 9 Platinum / Appearance Manager palette:
                                                            //   Desktop = #9999BB  (the iconic lavender / cool purple-gray)
                                                            //   Window  = #DDDDDD  (light gray pinstriped chrome)
                                                            //   Surface = #FFFFFF  (dialog / sheet body)
                                                            //   Body    = #000000  (text)
                                                            //   Accent  = #3366CC  (royal blue — the Apple Highlight Color
                                                            //                       default; used for default-button glow,
                                                            //                       selection, focus ring)
                                                            //   Hilite  = #B5CFEC  (selection background — pale variant
                                                            //                       of the highlight colour)
                                                            //
                                                            // Mapping:
                                                            //   AppBar     = WHITE  (the menu bar at the top of the screen)
                                                            //   Drawer     = LAVENDER (the desktop)
                                                            //   Surface    = WHITE  (cards / dialogs / Paper)
                                                            //   Background = LAVENDER (page body — desktop showing through)
                                                            //   Primary    = ROYAL BLUE (default-action / focused-control colour)
                                                            //   Secondary  = LAVENDER-LIGHT (calmer accent that fits the era)
                                                            //   Tertiary   = MID-GRAY (3D-bevel button gray for less-emphasised UI)
                                                            PaletteLight = new PaletteLight
                                                            {
                                                                Primary                  = "#3366CC",
                                                                PrimaryContrastText      = "#FFFFFF",
                                                                Secondary                = "#9999BB",
                                                                SecondaryContrastText    = "#FFFFFF",
                                                                Tertiary                 = "#888888",
                                                                TertiaryContrastText     = "#FFFFFF",
                                                                AppbarBackground         = "#FFFFFF",
                                                                AppbarText               = "#000000",
                                                                DrawerBackground         = "#9999BB",
                                                                DrawerText               = "#FFFFFF",
                                                                DrawerIcon               = "#FFFFFF",
                                                                Surface                  = "#FFFFFF",
                                                                Background               = "#9999BB",
                                                                BackgroundGray           = "#DDDDDD",
                                                                TextPrimary              = "#000000",
                                                                TextSecondary            = "#444466",
                                                                TextDisabled             = "#888899",
                                                                ActionDefault            = "#000000",
                                                                ActionDisabled           = "#888899",
                                                                ActionDisabledBackground = "#CCCCDD",
                                                                LinesDefault             = "#888899",
                                                                LinesInputs              = "#666677",
                                                                TableLines               = "#CCCCDD",
                                                                TableStriped             = "#EEEEF4",
                                                                TableHover               = "#B5CFEC",
                                                                Divider                  = "#888899",
                                                                DividerLight             = "#CCCCDD",
                                                                Info                     = "#3366CC",
                                                                Success                  = "#339966",
                                                                Warning                  = "#CC9933",
                                                                Error                    = "#CC3333",
                                                                Dark                     = "#000000",
                                                                HoverOpacity             = 0.08
                                                            },
                                                            LayoutProperties = new LayoutProperties
                                                            {
                                                                DrawerWidthLeft     = "260px",
                                                                DrawerMiniWidthLeft = "72px"
                                                            },
                                                            Typography = new Typography
                                                            {
                                                                // Body text uses Geneva/Helvetica fall-throughs since Virtue
                                                                // is a display face (the Charcoal recreation is best at
                                                                // larger sizes for headings / buttons / menu chrome).
                                                                Default = new DefaultTypography
                                                                {
                                                                    FontFamily    = ["Geneva", "Helvetica Neue", "Arial", "sans-serif"],
                                                                    FontSize      = "0.875rem",
                                                                    FontWeight    = "400",
                                                                    LineHeight    = "1.45",
                                                                    LetterSpacing = "0"
                                                                },
                                                                H1 = new H1Typography
                                                                {
                                                                    FontFamily = _chiKareGoStack,
                                                                    FontSize   = "2rem",
                                                                    FontWeight = "400"
                                                                },
                                                                H2 = new H2Typography
                                                                {
                                                                    FontFamily = _chiKareGoStack,
                                                                    FontSize   = "1.75rem",
                                                                    FontWeight = "400"
                                                                },
                                                                H3 = new H3Typography
                                                                {
                                                                    FontFamily = _chiKareGoStack,
                                                                    FontSize   = "1.5rem",
                                                                    FontWeight = "400"
                                                                },
                                                                H4 = new H4Typography
                                                                {
                                                                    FontFamily = _chiKareGoStack,
                                                                    FontSize   = "1.25rem",
                                                                    FontWeight = "400"
                                                                },
                                                                H5 = new H5Typography
                                                                {
                                                                    FontFamily = _chiKareGoStack,
                                                                    FontSize   = "1.125rem",
                                                                    FontWeight = "400"
                                                                },
                                                                H6 = new H6Typography
                                                                {
                                                                    FontFamily = _chiKareGoStack,
                                                                    FontSize   = "1rem",
                                                                    FontWeight = "400"
                                                                },
                                                                Subtitle1 = new Subtitle1Typography
                                                                {
                                                                    FontFamily = _chiKareGoStack,
                                                                    FontWeight = "400"
                                                                },
                                                                Subtitle2 = new Subtitle2Typography
                                                                {
                                                                    FontFamily = _chiKareGoStack,
                                                                    FontWeight = "400"
                                                                },
                                                                Body1 = new Body1Typography
                                                                {
                                                                    FontFamily = ["Geneva", "Helvetica Neue", "Arial", "sans-serif"]
                                                                },
                                                                Body2 = new Body2Typography
                                                                {
                                                                    FontFamily = ["Geneva", "Helvetica Neue", "Arial", "sans-serif"]
                                                                },
                                                                Button = new ButtonTypography
                                                                {
                                                                    FontFamily    = _chiKareGoStack,
                                                                    FontWeight    = "400",
                                                                    TextTransform = "none"
                                                                },
                                                                Caption = new CaptionTypography
                                                                {
                                                                    FontFamily = ["Geneva", "Helvetica Neue", "Arial", "sans-serif"]
                                                                },
                                                                Overline = new OverlineTypography
                                                                {
                                                                    FontFamily    = _chiKareGoStack,
                                                                    FontWeight    = "400",
                                                                    TextTransform = "uppercase"
                                                                }
                                                            }
                                                        },
                                                        _macOs9Fonts);

    /// <summary>All themes available to users in the Appearance picker. Order matters — it's the display order.</summary>
    public static readonly IReadOnlyList<ThemeDefinition> All = new[] { DefaultDark, DefaultLight, AmigaOs, Dos, MacOs9 };

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
