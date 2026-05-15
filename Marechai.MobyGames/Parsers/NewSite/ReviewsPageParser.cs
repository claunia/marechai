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
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using Marechai.MobyGames.Models;

namespace Marechai.MobyGames.Parsers.NewSite;

/// <summary>
///     Parses the new MobyGames Reviews sub-page (post-2023 Vue redesign).
///     Critic reviews live as a JSON array inside the Vue custom element
///     <c>&lt;critic-reviews :reviews='[{...},{...}]'&gt;</c>. Each entry is a
///     dictionary with <c>citation</c>, <c>date</c>, <c>score</c>,
///     <c>normalized_score</c>, <c>max_score</c>, <c>url</c>,
///     <c>platform</c>: {id, name}, <c>source</c>: {id, name, url, ...} — all
///     the fields the legacy HTML parser had to scrape out of nested divs.
/// </summary>
public static partial class ReviewsPageParser
{
    [GeneratedRegex(@"<critic-reviews\b[^>]*?:reviews='(?<json>(?:[^'\\]|\\.)*)'", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex CriticReviewsAttrRegex();

    public static List<ParsedCriticReview> Parse(string html)
    {
        var reviews = new List<ParsedCriticReview>();

        if(string.IsNullOrEmpty(html)) return reviews;

        Match m = CriticReviewsAttrRegex().Match(html);

        if(!m.Success) return reviews;

        string jsonEscaped = m.Groups["json"].Value;

        // The Vue attribute uses single-quoted HTML, so the embedded JSON contains
        // entity-escaped quotes ("). HtmlDecode unescapes them back to literal ".
        string json = WebUtility.HtmlDecode(jsonEscaped);

        // Vue/HTMLEntity decoding may leave behind backslash-escaped HTML wrappers
        // around the inner strings; the JSON itself remains valid after the decode.
        JsonElement root;

        try
        {
            using JsonDocument doc = JsonDocument.Parse(json);
            root = doc.RootElement.Clone();
        }
        catch(JsonException)
        {
            return reviews;
        }

        if(root.ValueKind != JsonValueKind.Array) return reviews;

        foreach(JsonElement entry in root.EnumerateArray())
        {
            if(entry.ValueKind != JsonValueKind.Object) continue;

            var review = new ParsedCriticReview();

            if(entry.TryGetProperty("citation", out JsonElement citation) &&
               citation.ValueKind == JsonValueKind.String)
                review.ReviewText = citation.GetString();

            if(entry.TryGetProperty("date", out JsonElement date) &&
               date.ValueKind == JsonValueKind.String)
                review.ReviewDate = date.GetString();

            if(entry.TryGetProperty("url", out JsonElement url) &&
               url.ValueKind == JsonValueKind.String)
                review.ReviewUrl = url.GetString();

            if(entry.TryGetProperty("normalized_score", out JsonElement norm) &&
               norm.ValueKind == JsonValueKind.Number &&
               norm.TryGetDouble(out double normVal))
                review.NormalizedScore = (int)Math.Round(normVal);

            if(entry.TryGetProperty("score", out JsonElement raw) &&
               raw.ValueKind == JsonValueKind.Number &&
               raw.TryGetDouble(out double rawVal))
                review.OriginalScore = (float)rawVal;

            if(entry.TryGetProperty("max_score", out JsonElement mx) &&
               mx.ValueKind == JsonValueKind.Number &&
               mx.TryGetDouble(out double mxVal))
                review.OriginalScoreMaximum = (float)mxVal;

            if(entry.TryGetProperty("platform", out JsonElement platform) &&
               platform.ValueKind == JsonValueKind.Object &&
               platform.TryGetProperty("name", out JsonElement pn) &&
               pn.ValueKind == JsonValueKind.String)
                review.PlatformName = pn.GetString();

            if(entry.TryGetProperty("source", out JsonElement source) &&
               source.ValueKind == JsonValueKind.Object)
            {
                if(source.TryGetProperty("name", out JsonElement sn) &&
                   sn.ValueKind == JsonValueKind.String)
                    review.PublicationName = sn.GetString();

                if(source.TryGetProperty("id", out JsonElement sid) &&
                   sid.ValueKind == JsonValueKind.Number &&
                   sid.TryGetInt32(out int sidVal))
                    review.PublicationSourceId = sidVal;
            }

            if(string.IsNullOrWhiteSpace(review.PublicationName) &&
               string.IsNullOrWhiteSpace(review.ReviewText))
                continue;

            reviews.Add(review);
        }

        return reviews;
    }
}
