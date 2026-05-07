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
using System.Threading.Tasks;

namespace Marechai.Theming;

/// <summary>
///     Per-circuit reactive holder for the active <see cref="ThemeDefinition" />. <see cref="MainLayout" /> subscribes
///     to <see cref="Changed" /> to re-render whenever the picker (or login) updates the theme. Initialised to
///     <see cref="ThemeCatalog.Default" /> so anonymous and prerender contexts have a sane value.
/// </summary>
public sealed class ThemeStateService
{
    public ThemeDefinition CurrentTheme { get; private set; } = ThemeCatalog.Default;

    public event Action Changed;

    /// <summary>Apply a known theme synchronously and notify subscribers. No-op when the theme is already current.</summary>
    public void Set(ThemeDefinition theme)
    {
        if(theme is null) theme = ThemeCatalog.Default;

        if(ReferenceEquals(theme, CurrentTheme)) return;

        CurrentTheme = theme;
        Changed?.Invoke();
    }

    /// <summary>
    ///     Resolve a slug via <see cref="ThemeCatalog.FindById" /> and apply, falling back to
    ///     <see cref="ThemeCatalog.Default" /> when the slug is unknown or null. Async signature kept for symmetry
    ///     with future server-side resolution.
    /// </summary>
    public Task SetByIdAsync(string id)
    {
        Set(ThemeCatalog.FindById(id) ?? ThemeCatalog.Default);

        return Task.CompletedTask;
    }
}
