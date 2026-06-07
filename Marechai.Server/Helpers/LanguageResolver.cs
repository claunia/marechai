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
using System.Linq;
using Marechai.Translation;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Marechai.Server.Helpers;

/// <summary>
///     Shared helper used by every translated endpoint to pick a 3-letter ISO 639-3 language code.
///     Resolution order: explicit <c>?lang=</c> query parameter → <c>Accept-Language</c> header
///     (BCP-47 → 3-letter mapping) → <c>"eng"</c>. Validates against
///     <see cref="TranslationService.IsLanguageSupported" /> so callers always receive a code the
///     translation cache can serve.
/// </summary>
public static class LanguageResolver
{
    /// <summary>
    ///     Resolves the request's preferred translation language. Safe to call with a null
    ///     <paramref name="httpContext" /> (returns <c>"eng"</c> when no header is available).
    /// </summary>
    public static string Resolve(HttpContext httpContext, string explicitLang)
    {
        if(!string.IsNullOrWhiteSpace(explicitLang))
        {
            string normalized = explicitLang.Trim().ToLowerInvariant();
            if(TranslationService.IsLanguageSupported(normalized)) return normalized;
        }

        string header = httpContext?.Request?.Headers.AcceptLanguage.ToString();
        if(string.IsNullOrWhiteSpace(header)) return "eng";

        IList<StringWithQualityHeaderValue> parsed;

        try
        {
            parsed = StringWithQualityHeaderValue.ParseList(new[] { header });
        }
        catch
        {
            return "eng";
        }

        foreach(StringWithQualityHeaderValue entry in parsed.OrderByDescending(e => e.Quality ?? 1.0))
        {
            string tag = entry.Value.Value;
            if(string.IsNullOrWhiteSpace(tag) || tag == "*") continue;

            // Use only the two-letter language part; ignore region (e.g. fr-CA → fr).
            int    dash      = tag.IndexOf('-');
            string twoLetter = (dash > 0 ? tag[..dash] : tag).ToLowerInvariant();

            string mapped = twoLetter switch
            {
                "en" => "eng",
                "es" => "spa",
                "de" => "deu",
                "fr" => "fra",
                "it" => "ita",
                "nl" => "nld",
                "pt" => "por",
                _    => null
            };

            if(mapped is not null && TranslationService.IsLanguageSupported(mapped)) return mapped;
        }

        return "eng";
    }
}
