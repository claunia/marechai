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

    /// <summary>All themes available to users in the Appearance picker. Order matters — it's the display order.</summary>
    public static readonly IReadOnlyList<ThemeDefinition> All = new[] { DefaultDark, DefaultLight };

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
