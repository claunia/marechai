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
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Marechai.Server.Localization;

/// <summary>
///     Picks the user's preferred culture for outgoing communications (currently 2FA emails) from the request's
///     <c>Accept-Language</c> header, restricted to the set Marechai supports across both Blazor and Uno
///     front-ends. Falls back to <c>en</c> when nothing matches.
/// </summary>
public static class EmailCulture
{
    /// <summary>
    ///     Cultures we ship localized email templates for. Order doesn't matter; lookup is by case-insensitive
    ///     match on either the full name or the two-letter language code.
    /// </summary>
    public static readonly string[] Supported = ["en", "es", "de", "fr", "it", "pt-BR"];

    static readonly HashSet<string> _supportedSet =
        new(Supported, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     Walks the request's <c>Accept-Language</c> header in priority order (q-value descending) and returns
    ///     the first language tag that matches one of <see cref="Supported" />. Matches by full BCP-47 tag first
    ///     (so <c>pt-BR</c> wins over <c>pt-PT</c>) and by two-letter code as a fallback (so <c>fr-CA</c> still
    ///     selects French). Returns <c>"en"</c> when nothing matches.
    /// </summary>
    public static CultureInfo PickFromRequest(HttpRequest request)
    {
        if(request is null) return CultureInfo.GetCultureInfo("en");

        string header = request.Headers.AcceptLanguage.ToString();
        if(string.IsNullOrWhiteSpace(header)) return CultureInfo.GetCultureInfo("en");

        IList<StringWithQualityHeaderValue> parsed;

        try
        {
            parsed = StringWithQualityHeaderValue.ParseList(new[] { header });
        }
        catch
        {
            return CultureInfo.GetCultureInfo("en");
        }

        foreach(StringWithQualityHeaderValue entry in parsed
                                                     .OrderByDescending(e => e.Quality ?? 1.0))
        {
            string tag = entry.Value.Value;
            if(string.IsNullOrWhiteSpace(tag) || tag == "*") continue;

            // Full tag match first (e.g. pt-BR).
            if(_supportedSet.Contains(tag)) return CultureInfo.GetCultureInfo(tag);

            // Two-letter language fallback (e.g. fr-CA -> fr).
            int dash = tag.IndexOf('-');

            if(dash > 0)
            {
                string lang = tag[..dash];
                if(_supportedSet.Contains(lang)) return CultureInfo.GetCultureInfo(lang);
            }
        }

        return CultureInfo.GetCultureInfo("en");
    }

    /// <summary>
    ///     Maps an arbitrary BCP-47 language tag (such as the value persisted in
    ///     <c>ApplicationUser.LastLanguageVisited</c>) onto the closest <see cref="Supported" /> email culture.
    ///     Falls back to <c>"en"</c> for null, empty, or unsupported inputs. Used by background workers (notably
    ///     <c>MessageNotificationWorker</c>) that must pick a culture without an inbound HTTP request.
    /// </summary>
    public static CultureInfo MapToSupported(string tag)
    {
        if(string.IsNullOrWhiteSpace(tag)) return CultureInfo.GetCultureInfo("en");

        // Full tag match first (e.g. pt-BR).
        if(_supportedSet.Contains(tag)) return CultureInfo.GetCultureInfo(tag);

        // Two-letter language fallback (e.g. fr-CA -> fr).
        int dash = tag.IndexOf('-');

        if(dash > 0)
        {
            string lang = tag[..dash];
            if(_supportedSet.Contains(lang)) return CultureInfo.GetCultureInfo(lang);
        }

        return CultureInfo.GetCultureInfo("en");
    }
}
