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
using Marechai.Data;

namespace Marechai.Helpers;

/// <summary>
///     Parses the optional "X/" prefix on a search query and returns a stripped query plus the
///     selected entity-type. Lets users restrict autocomplete to a single entity category, e.g.
///     <c>"c/amst"</c> → companies named "amst*".
///
///     Prefix table (must match the help footer rendered by SearchBar.razor):
///       <c>c/</c> Company, <c>m/</c> Computer, <c>cn/</c> Console, <c>sm/</c> Smartphone,
///       <c>pda/</c> Pda, <c>b/</c> Book, <c>d/</c> Document, <c>mg/</c> Magazine, <c>g/</c> Gpu,
///       <c>p/</c> Processor, <c>s/</c> SoundSynth, <c>pe/</c> Person, <c>sw/</c> Software,
///       <c>cl/</c> SoftwareCompilation.
/// </summary>
public static class SearchPrefixParser
{
    public static readonly IReadOnlyDictionary<string, SearchEntityType> Prefixes =
        new Dictionary<string, SearchEntityType>
        {
            ["c"]  = SearchEntityType.Company,
            ["m"]  = SearchEntityType.Computer,
            ["cn"] = SearchEntityType.Console,
            ["sm"] = SearchEntityType.Smartphone,
            ["pda"] = SearchEntityType.Pda,
            ["b"]  = SearchEntityType.Book,
            ["d"]  = SearchEntityType.Document,
            ["mg"] = SearchEntityType.Magazine,
            ["g"]  = SearchEntityType.Gpu,
            ["p"]  = SearchEntityType.Processor,
            ["s"]  = SearchEntityType.SoundSynth,
            ["pe"] = SearchEntityType.Person,
            ["sw"] = SearchEntityType.Software,
            ["cl"] = SearchEntityType.SoftwareCompilation
        };

    /// <summary>
    ///     Strips a known prefix from <paramref name="raw"/>. Returns <c>(query, entityType)</c>
    ///     where <c>entityType</c> is null when no prefix matches.
    /// </summary>
    public static (string query, SearchEntityType? entityType) Parse(string raw)
    {
        if(string.IsNullOrEmpty(raw)) return (raw ?? string.Empty, null);

        int slash = raw.IndexOf('/');
        if(slash <= 0 || slash > 3) return (raw, null);

        string prefix = raw[..slash].Trim().ToLowerInvariant();
        if(!Prefixes.TryGetValue(prefix, out SearchEntityType type)) return (raw, null);

        string rest = slash + 1 < raw.Length ? raw[(slash + 1)..] : string.Empty;
        return (rest, type);
    }
}
