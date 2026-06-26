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
///     Centralised constants and small helpers for the SEO meta tags emitted on every Blazor page.
///     Owns the canonical hostname, brand asset URLs (logo, OpenGraph social card), and the
///     UI-culture → BCP-47-style locale mapping used by <c>og:locale</c>.
/// </summary>
public static class SeoMeta
{
    /// <summary>Canonical production hostname; used to build absolute URLs for canonical, og:url, JSON-LD @id, etc.</summary>
    public const string CanonicalHost = "https://www.marechai.net";

    /// <summary>Absolute URL of the 512×512 PNG logo used in JSON-LD <c>Organization.logo</c>.</summary>
    public const string LogoUrl = CanonicalHost + "/img/marechai-logo-512.png";

    /// <summary>Absolute URL of the 1200×630 PNG OpenGraph social card.</summary>
    public const string OgImageUrl = CanonicalHost + "/img/marechai-og.png";

    /// <summary>Width (px) of <see cref="OgImageUrl" />, emitted as <c>og:image:width</c>.</summary>
    public const int OgImageWidth = 1200;

    /// <summary>Height (px) of <see cref="OgImageUrl" />, emitted as <c>og:image:height</c>.</summary>
    public const int OgImageHeight = 630;

    /// <summary>Site name emitted as <c>og:site_name</c> and used in the JSON-LD <c>WebSite.name</c>.</summary>
    public const string SiteName = "Marechai";

    /// <summary>Public GitHub repository URL emitted in the JSON-LD <c>Organization.sameAs</c> array.</summary>
    public const string GitHubUrl = "https://github.com/claunia/marechai";

    /// <summary>
    ///     Maps the current UI culture's two-letter ISO 639-1 code to a canonical BCP-47-style locale
    ///     suitable for <c>og:locale</c> (e.g. <c>en_US</c>, <c>es_ES</c>). Unknown cultures fall back
    ///     to <c>en_US</c>.
    /// </summary>
    public static string OgLocale() => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName switch
    {
        "en" => "en_US",
        "es" => "es_ES",
        "de" => "de_DE",
        "fr" => "fr_FR",
        "it" => "it_IT",
        "la" => "la_VA",
        "pt" => "pt_BR",
        _    => "en_US"
    };

    /// <summary>
    ///     Builds an absolute canonical URL by concatenating <see cref="CanonicalHost" /> with the supplied
    ///     site-relative path. The path is normalised so it always starts with a single <c>/</c>.
    ///     Pass <c>"/"</c> to get the canonical root URL.
    /// </summary>
    public static string BuildCanonicalUrl(string relativePath)
    {
        if(string.IsNullOrEmpty(relativePath)) return CanonicalHost + "/";

        return relativePath.StartsWith('/') ? CanonicalHost + relativePath : CanonicalHost + "/" + relativePath;
    }
}
