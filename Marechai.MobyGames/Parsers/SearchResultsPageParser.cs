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
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Marechai.MobyGames.Parsers;

/// <summary>
///     Parses the JSON payload embedded inside the new MobyGames search-results page's
///     <c>&lt;game-browser :initial-values='…'&gt;</c> Vue component attribute.
///     <para>
///         A search URL like
///         <c>https://www.mobygames.com/game/from:{year}/until:{year}/sort:title/page:{N}/</c>
///         returns a page whose <c>:initial-values</c> attribute (HTML-entity-encoded) decodes to
///         a JSON object of shape:
///     </para>
///     <code>
///     {
///         "baseUrl":  "/game/from:2020/.../",
///         "games":    [ { "game_id": 12345, "internal_url": "/game/12345/some-title/", "title": "...",
///                         "release_date": "2020-03-04", "companies": [ { "id": 9, "name": "Acme",
///                         "title_id": 1 } ] }, ... ],
///         "page":     3,
///         "perPage":  18,
///         "total":    1234,
///         "maxPages": 14    // anonymous; ~1000 when authenticated
///     }
///     </code>
///     <para>
///         This parser is intentionally tolerant: any missing field falls back to <c>null</c> or 0
///         so older / experimental site responses don't crash discovery.
///     </para>
/// </summary>
public static partial class SearchResultsPageParser
{
    [GeneratedRegex(@"^/?game/(\d+)/([^/]+)/?$", RegexOptions.IgnoreCase)]
    private static partial Regex GameInternalUrlRegex();

    /// <summary>
    ///     Matches the slug + numeric id segment of a MobyGames game URL. Used for parsing the
    ///     export endpoint's <c>moby_url</c> which is a FULL URL like
    ///     <c>https://www.mobygames.com/game/246066/007-first-light/</c>, not a relative path.
    /// </summary>
    [GeneratedRegex(@"/game/(\d+)/([^/]+)/?", RegexOptions.IgnoreCase)]
    private static partial Regex ExportMobyUrlRegex();

    /// <summary>
    ///     A single game row from the search-results JSON payload.
    /// </summary>
    public sealed record SearchResultGame(
        int       NumericId,
        string    Slug,
        string    Title,
        int?      ReleaseYear,
        string    Developer);

    /// <summary>
    ///     The decoded shape of one search-results page.
    /// </summary>
    public sealed record SearchResultsPage(
        int                                Page,
        int                                PerPage,
        int                                Total,
        int                                MaxPages,
        IReadOnlyList<SearchResultGame>    Games);

    /// <summary>
    ///     Decode the <c>:initial-values</c> JSON payload into a typed page.
    ///     Returns <c>null</c> when the input is empty or malformed.
    /// </summary>
    public static SearchResultsPage Parse(string initialValuesJson)
    {
        if(string.IsNullOrWhiteSpace(initialValuesJson))
            return null;

        try
        {
            using var doc = JsonDocument.Parse(initialValuesJson);
            JsonElement root = doc.RootElement;

            int page     = GetIntOr(root, "page",     1);
            int perPage  = GetIntOr(root, "perPage",  18);
            int total    = GetIntOr(root, "total",    0);
            int maxPages = GetIntOr(root, "maxPages", 0);

            var games = new List<SearchResultGame>();

            if(root.TryGetProperty("games", out JsonElement gamesEl) &&
               gamesEl.ValueKind == JsonValueKind.Array)
            {
                foreach(JsonElement g in gamesEl.EnumerateArray())
                {
                    SearchResultGame parsed = ParseGame(g);

                    if(parsed is not null)
                        games.Add(parsed);
                }
            }

            return new SearchResultsPage(page, perPage, total, maxPages, games);
        }
        catch(JsonException)
        {
            return null;
        }
    }

    /// <summary>
    ///     Decode the response from MobyGames' MobyPlus <c>?export=json</c> endpoint, which
    ///     returns ALL matching games for a year-filtered search as a flat JSON array — no
    ///     pagination metadata. Each element is a game object with shape
    ///     <code>
    ///     {
    ///         "id":            246066,
    ///         "title":         "007: First Light",
    ///         "release_date":  "2026"           // or "2026-03-31"
    ///         "developers":    [ "Io-Interactive A/S" ],
    ///         "publishers":    [ ... ],
    ///         "platforms":     [ ... ],
    ///         "genres":        [ ... ],
    ///         "moby_score":    null,
    ///         "moby_url":      "https://www.mobygames.com/game/246066/007-first-light/"
    ///     }
    ///     </code>
    ///     <para>
    ///         Returns <c>null</c> when the input is malformed (caller treats as fatal for the
    ///         year). Returns an empty list when the array is present but contains no entries
    ///         (treated as "no releases for this year" — non-fatal).
    ///     </para>
    /// </summary>
    public static IReadOnlyList<SearchResultGame> ParseExport(string exportJson)
    {
        if(string.IsNullOrWhiteSpace(exportJson))
            return null;

        try
        {
            using var doc = JsonDocument.Parse(exportJson);

            if(doc.RootElement.ValueKind != JsonValueKind.Array)
                return null;

            var games = new List<SearchResultGame>(doc.RootElement.GetArrayLength());

            foreach(JsonElement g in doc.RootElement.EnumerateArray())
            {
                SearchResultGame parsed = ParseExportGame(g);

                if(parsed is not null)
                    games.Add(parsed);
            }

            return games;
        }
        catch(JsonException)
        {
            return null;
        }
    }

    static SearchResultGame ParseExportGame(JsonElement g)
    {
        int    gameId  = GetIntOr(g,    "id",       0);
        string title   = GetStringOr(g, "title",    null);
        string mobyUrl = GetStringOr(g, "moby_url", null);

        // Slug + (fallback) id come from the moby_url; the export endpoint's `id` field is
        // authoritative when present, but moby_url is double-checked because some legacy entries
        // have id=0 / id=null and only the URL identifies the game.
        string slug = null;

        if(!string.IsNullOrEmpty(mobyUrl))
        {
            Match m = ExportMobyUrlRegex().Match(mobyUrl);

            if(m.Success)
            {
                if(gameId <= 0 &&
                   int.TryParse(m.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture,
                                out int idFromUrl))
                    gameId = idFromUrl;

                slug = m.Groups[2].Value;
            }
        }

        if(gameId <= 0)
            return null;

        // release_date is either "YYYY" or "YYYY-MM-DD"; year is the leading 4 chars.
        int?   releaseYear = null;
        string releaseDate = GetStringOr(g, "release_date", null);

        if(!string.IsNullOrEmpty(releaseDate) &&
           releaseDate.Length >= 4 &&
           int.TryParse(releaseDate[..4], NumberStyles.Integer, CultureInfo.InvariantCulture, out int year))
            releaseYear = year;

        // Developer = first non-empty entry in the `developers` string array (the export endpoint
        // has already filtered companies by role, so we don't need the title_id=1 pivot from the
        // SSR <game-browser> envelope).
        string developer = null;

        if(g.TryGetProperty("developers", out JsonElement devs) && devs.ValueKind == JsonValueKind.Array)
        {
            foreach(JsonElement d in devs.EnumerateArray())
            {
                if(d.ValueKind != JsonValueKind.String) continue;

                developer = d.GetString();

                if(!string.IsNullOrWhiteSpace(developer))
                    break;
            }
        }

        return new SearchResultGame(gameId, slug ?? string.Empty, title ?? string.Empty, releaseYear,
                                    developer ?? string.Empty);
    }

    static SearchResultGame ParseGame(JsonElement g)
    {
        int gameId = GetIntOr(g, "game_id", 0);

        string internalUrl = GetStringOr(g, "internal_url", null);
        string title       = GetStringOr(g, "title",        null);

        // Extract slug from internal_url (preferred). Fall back to game_id-only if missing.
        string slug = null;

        if(!string.IsNullOrEmpty(internalUrl))
        {
            Match m = GameInternalUrlRegex().Match(internalUrl);

            if(m.Success)
            {
                if(gameId <= 0 &&
                   int.TryParse(m.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture,
                                out int idFromUrl))
                    gameId = idFromUrl;

                slug = m.Groups[2].Value;
            }
        }

        if(gameId <= 0) return null;

        // ReleaseYear is parsed from "release_date" which is a YYYY-MM-DD string (or sometimes just YYYY).
        int? releaseYear = null;
        string releaseDate = GetStringOr(g, "release_date", null);

        if(!string.IsNullOrEmpty(releaseDate) && releaseDate.Length >= 4 &&
           int.TryParse(releaseDate[..4], NumberStyles.Integer, CultureInfo.InvariantCulture,
                        out int year))
            releaseYear = year;

        // Developer = first company with title_id == 1.
        string developer = null;

        if(g.TryGetProperty("companies", out JsonElement companies) &&
           companies.ValueKind == JsonValueKind.Array)
        {
            foreach(JsonElement c in companies.EnumerateArray())
            {
                if(GetIntOr(c, "title_id", 0) != 1) continue;

                developer = GetStringOr(c, "name", null);

                if(!string.IsNullOrWhiteSpace(developer))
                    break;
            }
        }

        return new SearchResultGame(gameId, slug ?? string.Empty, title ?? string.Empty, releaseYear,
                                    developer ?? string.Empty);
    }

    static int GetIntOr(JsonElement el, string prop, int defaultValue)
    {
        if(!el.TryGetProperty(prop, out JsonElement v)) return defaultValue;

        switch(v.ValueKind)
        {
            case JsonValueKind.Number:
                return v.TryGetInt32(out int i) ? i : defaultValue;
            case JsonValueKind.String:
                return int.TryParse(v.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture,
                                    out int parsed)
                           ? parsed
                           : defaultValue;
            default:
                return defaultValue;
        }
    }

    static string GetStringOr(JsonElement el, string prop, string defaultValue)
    {
        if(!el.TryGetProperty(prop, out JsonElement v)) return defaultValue;

        return v.ValueKind switch
        {
            JsonValueKind.String => v.GetString(),
            JsonValueKind.Number => v.GetRawText(),
            _                    => defaultValue
        };
    }
}
