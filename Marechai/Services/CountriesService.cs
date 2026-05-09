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

namespace Marechai.Services;

/// <summary>
///     Marker type that exists solely to drive a centralized
///     <see cref="Microsoft.Extensions.Localization.IStringLocalizer{T}"/>
///     for UN M49 country and region names. The accompanying
///     <c>CountriesService.{en,es,de,fr,it}.resx</c> files contain the
///     translations for every country and macro-region exposed by the API.
///
///     Pages that display country or region names (for example the software
///     release view, the by-country company list, the by-country people list)
///     should inject <c>IStringLocalizer&lt;CountriesService&gt;</c> instead of
///     duplicating those translations inside their own page-specific resx
///     files.
/// </summary>
public sealed class CountriesService;
