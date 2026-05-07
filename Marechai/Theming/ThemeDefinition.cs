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

using System.Collections.Generic;
using MudBlazor;

namespace Marechai.Theming;

/// <summary>
///     Single named entry in the <see cref="ThemeCatalog" />. Carries the slug used for persistence, a human-readable
///     display name, the dark/light flag passed to <c>MudThemeProvider.IsDarkMode</c>, the actual
///     <see cref="MudBlazor.MudTheme" /> instance with its full palette + typography, and an optional list of CSS /
///     web-font URLs that <see cref="ThemeFontLoader" /> will inject into <c>&lt;head&gt;</c> when this theme is
///     active.
/// </summary>
public sealed record ThemeDefinition(string Id, string DisplayName, bool IsDark, MudTheme MudTheme,
                                     IReadOnlyList<string> WebFontUrls);
