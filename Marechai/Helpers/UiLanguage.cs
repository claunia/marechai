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

using System.Globalization;

namespace Marechai.Helpers;

/// <summary>
///     Maps the Blazor request UI culture (<see cref="CultureInfo.CurrentUICulture" />, set per-request via
///     <c>CookieRequestCultureProvider</c> in <c>_Host.cshtml.cs</c>) to the ISO 639-3 three-letter language
///     codes used by Marechai's per-language description tables and the related <c>/{entity}/{id}/full</c> +
///     <c>/{entity}/{id}/description{,/text}</c> endpoints.
///
///     The Blazor side currently exposes 6 cultures (en-US, es, de, fr, it, nl — see
///     <c>Startup._supportedCultures</c>); the description tables additionally support <c>lat</c> and
///     <c>por</c>, which never come out of this helper because the UI doesn't expose those locales.
///
///     Every entity view that fetches a description from the API MUST call this and forward the result so
///     the user sees their preferred translation (with English fallback resolved server-side). Calling the
///     description endpoints without a <c>lang</c> argument silently always returns English, which is the
///     bug this helper is here to prevent.
/// </summary>
public static class UiLanguage
{
    /// <summary>
    ///     Returns the ISO 639-3 code for the current request's UI culture, or <c>"eng"</c> when the culture
    ///     is unknown / unmapped. Lookup is by the culture's two-letter ISO 639-1 code so culture variants
    ///     (es-ES, fr-CA, etc.) all resolve correctly.
    /// </summary>
    public static string GetIso639_3() => MapTwoLetterToThree(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);

    static string MapTwoLetterToThree(string twoLetter) => twoLetter switch
    {
        "en" => "eng",
        "es" => "spa",
        "de" => "deu",
        "fr" => "fra",
        "it" => "ita",
        "pt" => "por",
        _    => "eng"
    };
}
