/*******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ---------------------------------------------------------------------------
//
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ License ] -----------------------------------------------------------
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
// ---------------------------------------------------------------------------
// Copyright © 2003-2026 Natalia Portillo
*******************************************************************************/

using System;
using System.Collections.Generic;

namespace Marechai.Data.Constants;

/// <summary>
///     Server-side allow-list of UI theme slugs accepted by <c>PUT /auth/me/theme</c>. The client-side
///     <c>ThemeCatalog</c> lives in the Blazor project (it carries the actual <c>MudTheme</c> instances) and must be
///     kept in sync with this list. Adding a new theme is a two-line change: append the slug constant here, then add
///     the matching <c>ThemeDefinition</c> to <c>ThemeCatalog</c>.
/// </summary>
public static class ThemeIds
{
    /// <summary>The historical Marechai dark purple palette. Default for users with no preference set.</summary>
    public const string DefaultDark = "default-dark";

    /// <summary>Stock MudBlazor light palette.</summary>
    public const string DefaultLight = "default-light";

    /// <summary>AmigaOS Workbench 1.x palette (blue / white / black / orange) with the Topaz bitmap font.</summary>
    public const string AmigaOs = "amigaos";

    /// <summary>Borland-era DOS text-mode UI (blue desktop / gray windows / cyan accents) with the IBM VGA 9x16 font.</summary>
    public const string Dos = "dos";

    /// <summary>All recognised theme slugs. Used by the controller to validate incoming theme ids.</summary>
    public static readonly IReadOnlySet<string> All =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { DefaultDark, DefaultLight, AmigaOs, Dos };
}
