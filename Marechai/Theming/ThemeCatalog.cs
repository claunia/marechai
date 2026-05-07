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

    static readonly string[] _dejaVuCdeStack = ["DejaVu Sans CDE", "DejaVu Sans", "Lucida Grande", "Lucida Sans", "sans-serif"];

    static readonly IReadOnlyList<string> _cdeFonts = ["/css/themes/cde.css"];

    static readonly string[] _orbitronStack = ["Orbitron", "Eurostile", "Helvetica Neue", "Arial", "sans-serif"];

    static readonly IReadOnlyList<string> _cyberpunkFonts = ["/css/themes/cyberpunk.css"];

    static readonly string[] _vt323Stack = ["VT323", "Courier New", "Consolas", "monospace"];

    static readonly IReadOnlyList<string> _phosphorFonts = ["/css/themes/phosphor.css"];

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

    /// <summary>
    ///     CDE (Common Desktop Environment) — the classic UNIX workstation desktop from Sun, HP, and IBM.
    ///     Teal-gray Motif-style 3D bevels, dark teal title bars and panel, warm gray surfaces, and
    ///     DejaVu Sans as the closest free substitute for the Lucida Sans font used by the original
    ///     Solaris CDE. Inspired by CDE 1.x on Solaris 2.x (mid-1990s).
    /// </summary>
    public static readonly ThemeDefinition Cde = new(ThemeIds.Cde,
                                                     "CDE (Common Desktop)",
                                                     false,
                                                     new MudTheme
                                                     {
                                                         // CDE / Motif palette (from the Solaris default colour set):
                                                         //   Desktop bg    = #7B9494  (blue-gray — the desktop workspace)
                                                         //   Window chrome = #AEB2B2  (warm medium gray — Motif 3D buttons)
                                                         //   Title bar     = #D6A564  (warm tan/sandy — active window title,
                                                         //                             the signature CDE colour)
                                                         //   Content area  = #D9D9D9  (light gray — pane backgrounds)
                                                         //   Input field   = #FFFFFF  (white)
                                                         //   Text          = #000000  (black)
                                                         //   Selection     = #4D7B8A  (muted teal-blue)
                                                         //   Inactive      = #5F7070  (darker blue-gray)
                                                         //
                                                         // Mapping:
                                                         //   AppBar     = TAN     (the iconic CDE title-bar colour — most
                                                         //                         recognisable element, suitable for the top bar)
                                                         //   Drawer     = BLUE-GRAY (the desktop workspace)
                                                         //   Surface    = LIGHT GRAY (cards / dialogs)
                                                         //   Background = BLUE-GRAY (page body = desktop)
                                                         //   Primary    = MUTED TEAL (selection, focus ring, toggled controls)
                                                         //   Secondary  = TAN        (secondary accent matching CDE active chrome)
                                                         //   Tertiary   = WARM GRAY  (Motif 3D button faces)
                                                         PaletteLight = new PaletteLight
                                                         {
                                                             Primary                  = "#4D7B8A",
                                                             PrimaryContrastText      = "#FFFFFF",
                                                             Secondary                = "#D6A564",
                                                             SecondaryContrastText    = "#000000",
                                                             Tertiary                 = "#AEB2B2",
                                                             TertiaryContrastText     = "#000000",
                                                             AppbarBackground         = "#D6A564",
                                                             AppbarText               = "#000000",
                                                             DrawerBackground         = "#7B9494",
                                                             DrawerText               = "#FFFFFF",
                                                             DrawerIcon               = "#FFFFFF",
                                                             Surface                  = "#D9D9D9",
                                                             Background               = "#7B9494",
                                                             BackgroundGray           = "#AEB2B2",
                                                             TextPrimary              = "#000000",
                                                             TextSecondary            = "#333333",
                                                             TextDisabled             = "#777777",
                                                             ActionDefault            = "#000000",
                                                             ActionDisabled           = "#777777",
                                                             ActionDisabledBackground = "#BFBFBF",
                                                             LinesDefault             = "#777777",
                                                             LinesInputs              = "#555555",
                                                             TableLines               = "#BBBBBB",
                                                             TableStriped             = "#CCCCCC",
                                                             TableHover               = "#B8D0D6",
                                                             Divider                  = "#999999",
                                                             DividerLight             = "#CCCCCC",
                                                             Info                     = "#4D7B8A",
                                                             Success                  = "#5A8A5A",
                                                             Warning                  = "#D6A564",
                                                             Error                    = "#AA4444",
                                                             Dark                     = "#333333",
                                                             HoverOpacity             = 0.08
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
                                                                 FontFamily    = _dejaVuCdeStack,
                                                                 FontSize      = "0.875rem",
                                                                 FontWeight    = "400",
                                                                 LineHeight    = "1.45",
                                                                 LetterSpacing = "0"
                                                             },
                                                             H1 = new H1Typography
                                                             {
                                                                 FontFamily = _dejaVuCdeStack,
                                                                 FontSize   = "2rem",
                                                                 FontWeight = "700"
                                                             },
                                                             H2 = new H2Typography
                                                             {
                                                                 FontFamily = _dejaVuCdeStack,
                                                                 FontSize   = "1.75rem",
                                                                 FontWeight = "700"
                                                             },
                                                             H3 = new H3Typography
                                                             {
                                                                 FontFamily = _dejaVuCdeStack,
                                                                 FontSize   = "1.5rem",
                                                                 FontWeight = "700"
                                                             },
                                                             H4 = new H4Typography
                                                             {
                                                                 FontFamily = _dejaVuCdeStack,
                                                                 FontSize   = "1.25rem",
                                                                 FontWeight = "700"
                                                             },
                                                             H5 = new H5Typography
                                                             {
                                                                 FontFamily = _dejaVuCdeStack,
                                                                 FontSize   = "1.125rem",
                                                                 FontWeight = "700"
                                                             },
                                                             H6 = new H6Typography
                                                             {
                                                                 FontFamily = _dejaVuCdeStack,
                                                                 FontSize   = "1rem",
                                                                 FontWeight = "700"
                                                             },
                                                             Subtitle1 = new Subtitle1Typography
                                                             {
                                                                 FontFamily = _dejaVuCdeStack,
                                                                 FontWeight = "700"
                                                             },
                                                             Subtitle2 = new Subtitle2Typography
                                                             {
                                                                 FontFamily = _dejaVuCdeStack,
                                                                 FontWeight = "700"
                                                             },
                                                             Body1 = new Body1Typography
                                                             {
                                                                 FontFamily = _dejaVuCdeStack
                                                             },
                                                             Body2 = new Body2Typography
                                                             {
                                                                 FontFamily = _dejaVuCdeStack
                                                             },
                                                             Button = new ButtonTypography
                                                             {
                                                                 FontFamily    = _dejaVuCdeStack,
                                                                 FontWeight    = "700",
                                                                 TextTransform = "none"
                                                             },
                                                             Caption = new CaptionTypography
                                                             {
                                                                 FontFamily = _dejaVuCdeStack
                                                             },
                                                             Overline = new OverlineTypography
                                                             {
                                                                 FontFamily    = _dejaVuCdeStack,
                                                                 FontWeight    = "700",
                                                                 TextTransform = "uppercase"
                                                             }
                                                         }
                                                     },
                                                     _cdeFonts);

    /// <summary>
    ///     Cyberpunk — the original 1980s vision: deep purple-black backgrounds with hot magenta and electric
    ///     cyan neon accents, geometric futurist typography. Inspired by Blade Runner (1982), William Gibson's
    ///     Neuromancer (1984), Akira (1988), and the synthwave / outrun aesthetic that grew out of MTV-era
    ///     Miami Vice and arcade neon. NOT a reference to any modern video game.
    /// </summary>
    public static readonly ThemeDefinition Cyberpunk = new(ThemeIds.Cyberpunk,
                                                           "Cyberpunk (1980s)",
                                                           true,
                                                           new MudTheme
                                                           {
                                                               // 1980s cyberpunk neon palette:
                                                               //   Background = #0A0014 (near-black with deep purple tint —
                                                               //                         the night-city void)
                                                               //   Surface    = #15082A (slightly lighter purple-black for cards)
                                                               //   AppBar     = #1A0033 (deeper plum for the top chrome)
                                                               //   Drawer     = #100020 (sidebar — between AppBar and Surface)
                                                               //
                                                               //   Magenta    = #FF1493 (hot pink — the iconic 80s neon, used
                                                               //                         everywhere: Miami Vice, Blade Runner
                                                               //                         signage, vaporwave precursor era)
                                                               //   Cyan       = #00F0FF (electric cyan — the second half of
                                                               //                         the iconic 80s magenta+cyan pairing)
                                                               //   Purple     = #BD00FF (neon violet — used for tertiary
                                                               //                         accents and selection)
                                                               //   Green      = #39FF14 (toxic / lime neon — period-correct
                                                               //                         success colour, evokes CRT phosphor)
                                                               //   Yellow     = #FFD300 (amber neon — warnings)
                                                               //   Red        = #FF003C (vivid alert red — errors)
                                                               //
                                                               //   Text       = #F0EFFF (off-white with cool purple cast,
                                                               //                         feels like phosphor glow)
                                                               //   Muted text = #A09AC9 (lavender-gray)
                                                               PaletteDark = new PaletteDark
                                                               {
                                                                   Primary                  = "#FF1493",
                                                                   PrimaryContrastText      = "#0A0014",
                                                                   Secondary                = "#00F0FF",
                                                                   SecondaryContrastText    = "#0A0014",
                                                                   Tertiary                 = "#BD00FF",
                                                                   TertiaryContrastText     = "#FFFFFF",
                                                                   AppbarBackground         = "#1A0033",
                                                                   AppbarText               = "#00F0FF",
                                                                   DrawerBackground         = "#100020",
                                                                   DrawerText               = "#F0EFFF",
                                                                   DrawerIcon               = "#FF1493",
                                                                   Surface                  = "#15082A",
                                                                   Background               = "#0A0014",
                                                                   BackgroundGray           = "#1F0F36",
                                                                   TextPrimary              = "#F0EFFF",
                                                                   TextSecondary            = "#A09AC9",
                                                                   TextDisabled             = "#5A4F7C",
                                                                   ActionDefault            = "#00F0FF",
                                                                   ActionDisabled           = "#5A4F7C",
                                                                   ActionDisabledBackground = "#1F0F36",
                                                                   LinesDefault             = "#3D2A5C",
                                                                   LinesInputs              = "#FF1493",
                                                                   TableLines               = "#3D2A5C",
                                                                   TableStriped             = "#1F0F36",
                                                                   TableHover               = "#2A1450",
                                                                   Divider                  = "#3D2A5C",
                                                                   DividerLight             = "#5A4F7C",
                                                                   Info                     = "#00F0FF",
                                                                   Success                  = "#39FF14",
                                                                   Warning                  = "#FFD300",
                                                                   Error                    = "#FF003C",
                                                                   Dark                     = "#0A0014",
                                                                   HoverOpacity             = 0.15
                                                               },
                                                               LayoutProperties = new LayoutProperties
                                                               {
                                                                   DrawerWidthLeft     = "260px",
                                                                   DrawerMiniWidthLeft = "72px"
                                                               },
                                                               Typography = new Typography
                                                               {
                                                                   // Orbitron's geometric letterforms work well at all
                                                                   // sizes; use it everywhere for full sci-fi vibe.
                                                                   Default = new DefaultTypography
                                                                   {
                                                                       FontFamily    = _orbitronStack,
                                                                       FontSize      = "0.875rem",
                                                                       FontWeight    = "400",
                                                                       LineHeight    = "1.5",
                                                                       LetterSpacing = "0.02em"
                                                                   },
                                                                   H1 = new H1Typography
                                                                   {
                                                                       FontFamily    = _orbitronStack,
                                                                       FontSize      = "2.25rem",
                                                                       FontWeight    = "700",
                                                                       LetterSpacing = "0.05em"
                                                                   },
                                                                   H2 = new H2Typography
                                                                   {
                                                                       FontFamily    = _orbitronStack,
                                                                       FontSize      = "1.875rem",
                                                                       FontWeight    = "700",
                                                                       LetterSpacing = "0.05em"
                                                                   },
                                                                   H3 = new H3Typography
                                                                   {
                                                                       FontFamily    = _orbitronStack,
                                                                       FontSize      = "1.5rem",
                                                                       FontWeight    = "700",
                                                                       LetterSpacing = "0.04em"
                                                                   },
                                                                   H4 = new H4Typography
                                                                   {
                                                                       FontFamily    = _orbitronStack,
                                                                       FontSize      = "1.25rem",
                                                                       FontWeight    = "700",
                                                                       LetterSpacing = "0.03em"
                                                                   },
                                                                   H5 = new H5Typography
                                                                   {
                                                                       FontFamily    = _orbitronStack,
                                                                       FontSize      = "1.125rem",
                                                                       FontWeight    = "700",
                                                                       LetterSpacing = "0.03em"
                                                                   },
                                                                   H6 = new H6Typography
                                                                   {
                                                                       FontFamily    = _orbitronStack,
                                                                       FontSize      = "1rem",
                                                                       FontWeight    = "700",
                                                                       LetterSpacing = "0.03em"
                                                                   },
                                                                   Subtitle1 = new Subtitle1Typography
                                                                   {
                                                                       FontFamily    = _orbitronStack,
                                                                       FontWeight    = "700",
                                                                       LetterSpacing = "0.02em"
                                                                   },
                                                                   Subtitle2 = new Subtitle2Typography
                                                                   {
                                                                       FontFamily    = _orbitronStack,
                                                                       FontWeight    = "700",
                                                                       LetterSpacing = "0.02em"
                                                                   },
                                                                   Body1 = new Body1Typography
                                                                   {
                                                                       FontFamily = _orbitronStack
                                                                   },
                                                                   Body2 = new Body2Typography
                                                                   {
                                                                       FontFamily = _orbitronStack
                                                                   },
                                                                   Button = new ButtonTypography
                                                                   {
                                                                       FontFamily    = _orbitronStack,
                                                                       FontWeight    = "700",
                                                                       LetterSpacing = "0.08em",
                                                                       TextTransform = "uppercase"
                                                                   },
                                                                   Caption = new CaptionTypography
                                                                   {
                                                                       FontFamily = _orbitronStack
                                                                   },
                                                                   Overline = new OverlineTypography
                                                                   {
                                                                       FontFamily    = _orbitronStack,
                                                                       FontWeight    = "700",
                                                                       LetterSpacing = "0.15em",
                                                                       TextTransform = "uppercase"
                                                                   }
                                                               }
                                                           },
                                                           _cyberpunkFonts);

    /// <summary>
    ///     Phosphor — the green-on-black CRT terminal aesthetic of late-70s / 80s serial terminals
    ///     (DEC VT100/VT220/VT320, IBM 3270 in green-screen mode, the iconic "hacking scene" look from
    ///     WarGames, Alien, and every system-administrator's actual workstation through the 1980s).
    ///     Pure black background, brilliant P1-phosphor green text, single-weight VT323 monospace
    ///     reproducing the original DEC character ROM. Hierarchy is conveyed through brightness
    ///     levels of green rather than hue, matching how monochrome CRTs actually worked.
    /// </summary>
    public static readonly ThemeDefinition Phosphor = new(ThemeIds.Phosphor,
                                                          "Phosphor (Green CRT)",
                                                          true,
                                                          new MudTheme
                                                          {
                                                              // Monochrome green-phosphor palette — derived from
                                                              // the P1 phosphor used on DEC VT-series terminals:
                                                              //   Background = #000000 (CRT off / scanline gaps)
                                                              //   Bright fg  = #33FF66 (full-intensity phosphor green)
                                                              //   Mid fg     = #00CC33 (60% intensity — body text)
                                                              //   Dim fg     = #00802B (35% intensity — secondary text)
                                                              //   Faint      = #004D1A (15% intensity — disabled / lines)
                                                              //   Glow tint  = rgba(51,255,102,0.15) (hover / hilight)
                                                              //
                                                              // No hue accents — real green-phosphor terminals had
                                                              // exactly ONE colour. Status colours preserve their
                                                              // semantic meaning by using DIFFERENT BRIGHTNESS LEVELS
                                                              // (success = brightest, warning = mid + amber tint to
                                                              // hint at the optional amber-phosphor terminals, error
                                                              // = a touch of red because some VT241 colour terminals
                                                              // *did* have red for alerts).
                                                              //
                                                              // Mapping:
                                                              //   AppBar     = BLACK with bright-green text (status line)
                                                              //   Drawer     = BLACK with mid-green text (menu list)
                                                              //   Surface    = BLACK (windows are just framed regions)
                                                              //   Background = BLACK (the CRT itself)
                                                              //   Primary    = BRIGHT GREEN (focused/selected — the
                                                              //                              cursor's home colour)
                                                              //   Secondary  = MID GREEN (alternate accent)
                                                              //   Tertiary   = DIM GREEN (least-emphasised)
                                                              PaletteDark = new PaletteDark
                                                              {
                                                                  Primary                  = "#33FF66",
                                                                  PrimaryContrastText      = "#000000",
                                                                  Secondary                = "#00CC33",
                                                                  SecondaryContrastText    = "#000000",
                                                                  Tertiary                 = "#00802B",
                                                                  TertiaryContrastText     = "#000000",
                                                                  AppbarBackground         = "#000000",
                                                                  AppbarText               = "#33FF66",
                                                                  DrawerBackground         = "#000000",
                                                                  DrawerText               = "#00CC33",
                                                                  DrawerIcon               = "#33FF66",
                                                                  Surface                  = "#000000",
                                                                  Background               = "#000000",
                                                                  BackgroundGray           = "#0A1A0A",
                                                                  TextPrimary              = "#33FF66",
                                                                  TextSecondary            = "#00CC33",
                                                                  TextDisabled             = "#00802B",
                                                                  ActionDefault            = "#33FF66",
                                                                  ActionDisabled           = "#00802B",
                                                                  ActionDisabledBackground = "#0A1A0A",
                                                                  // Bright green frames everywhere — every "window"
                                                                  // on a real terminal was drawn with line-drawing
                                                                  // characters of the same bright phosphor.
                                                                  LinesDefault             = "#00CC33",
                                                                  LinesInputs              = "#33FF66",
                                                                  TableLines               = "#00802B",
                                                                  TableStriped             = "#0A1A0A",
                                                                  TableHover               = "#0F2A0F",
                                                                  Divider                  = "#00CC33",
                                                                  DividerLight             = "#00802B",
                                                                  Info                     = "#33FF66",
                                                                  Success                  = "#00FF66",
                                                                  // Amber tint nods to the optional amber-phosphor
                                                                  // terminals (some Wyse/IBM 3151 models shipped
                                                                  // with amber CRTs instead of green).
                                                                  Warning                  = "#FFB000",
                                                                  Error                    = "#FF3333",
                                                                  Dark                     = "#000000",
                                                                  HoverOpacity             = 0.18
                                                              },
                                                              LayoutProperties = new LayoutProperties
                                                              {
                                                                  DrawerWidthLeft     = "260px",
                                                                  DrawerMiniWidthLeft = "72px"
                                                              },
                                                              Typography = new Typography
                                                              {
                                                                  // VT323 has only one weight (it's a bitmap-style
                                                                  // monospace recreation of the VT320 character ROM).
                                                                  // Use it everywhere; differentiate hierarchy via
                                                                  // size + brightness, not weight.
                                                                  // Slight letter-spacing widening on overlines /
                                                                  // headings for the chunky CRT-readable feel.
                                                                  Default = new DefaultTypography
                                                                  {
                                                                      FontFamily    = _vt323Stack,
                                                                      FontSize      = "1rem",
                                                                      FontWeight    = "400",
                                                                      LineHeight    = "1.4",
                                                                      LetterSpacing = "0"
                                                                  },
                                                                  H1 = new H1Typography
                                                                  {
                                                                      FontFamily    = _vt323Stack,
                                                                      FontSize      = "2.5rem",
                                                                      FontWeight    = "400",
                                                                      LetterSpacing = "0.05em"
                                                                  },
                                                                  H2 = new H2Typography
                                                                  {
                                                                      FontFamily    = _vt323Stack,
                                                                      FontSize      = "2rem",
                                                                      FontWeight    = "400",
                                                                      LetterSpacing = "0.05em"
                                                                  },
                                                                  H3 = new H3Typography
                                                                  {
                                                                      FontFamily    = _vt323Stack,
                                                                      FontSize      = "1.6rem",
                                                                      FontWeight    = "400",
                                                                      LetterSpacing = "0.04em"
                                                                  },
                                                                  H4 = new H4Typography
                                                                  {
                                                                      FontFamily    = _vt323Stack,
                                                                      FontSize      = "1.3rem",
                                                                      FontWeight    = "400",
                                                                      LetterSpacing = "0.04em"
                                                                  },
                                                                  H5 = new H5Typography
                                                                  {
                                                                      FontFamily    = _vt323Stack,
                                                                      FontSize      = "1.15rem",
                                                                      FontWeight    = "400",
                                                                      LetterSpacing = "0.03em"
                                                                  },
                                                                  H6 = new H6Typography
                                                                  {
                                                                      FontFamily    = _vt323Stack,
                                                                      FontSize      = "1rem",
                                                                      FontWeight    = "400",
                                                                      LetterSpacing = "0.03em"
                                                                  },
                                                                  Subtitle1 = new Subtitle1Typography
                                                                  {
                                                                      FontFamily = _vt323Stack,
                                                                      FontWeight = "400"
                                                                  },
                                                                  Subtitle2 = new Subtitle2Typography
                                                                  {
                                                                      FontFamily = _vt323Stack,
                                                                      FontWeight = "400"
                                                                  },
                                                                  Body1 = new Body1Typography
                                                                  {
                                                                      FontFamily = _vt323Stack
                                                                  },
                                                                  Body2 = new Body2Typography
                                                                  {
                                                                      FontFamily = _vt323Stack
                                                                  },
                                                                  Button = new ButtonTypography
                                                                  {
                                                                      FontFamily    = _vt323Stack,
                                                                      FontWeight    = "400",
                                                                      LetterSpacing = "0.1em",
                                                                      TextTransform = "uppercase"
                                                                  },
                                                                  Caption = new CaptionTypography
                                                                  {
                                                                      FontFamily = _vt323Stack
                                                                  },
                                                                  Overline = new OverlineTypography
                                                                  {
                                                                      FontFamily    = _vt323Stack,
                                                                      FontWeight    = "400",
                                                                      LetterSpacing = "0.18em",
                                                                      TextTransform = "uppercase"
                                                                  }
                                                              }
                                                          },
                                                          _phosphorFonts);

    /// <summary>All themes available to users in the Appearance picker. Order matters — it's the display order.</summary>
    public static readonly IReadOnlyList<ThemeDefinition> All = new[] { DefaultDark, DefaultLight, AmigaOs, Cde, Cyberpunk, Dos, MacOs9, Phosphor };

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
