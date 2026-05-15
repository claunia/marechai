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
using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Marechai.MobyGames.Models;

namespace Marechai.MobyGames.Parsers.NewSite;

/// <summary>
///     Parses the new MobyGames Covers sub-page (post-2023 Vue redesign).
///     Each platform group is introduced by an <c>&lt;h2&gt;</c>, optionally
///     followed by a metadata <c>&lt;table class="table mb"&gt;</c> with
///     Packaging / Video Standard / Country rows, then one or more
///     <c>&lt;figure&gt;</c>s containing the cover thumbnail anchor and a
///     <c>&lt;figcaption&gt;</c> label like "Front Cover".
///     <para>
///         The h2 frequently joins several platforms (e.g. "Windows Apps and
///         Xbox One and Xbox Series") because MobyGames shares one cover
///         group across several SKUs. We split on " and " and emit one
///         <see cref="ParsedCoverGroup" /> per platform, all carrying the
///         same cover URLs. The downstream
///         <see cref="Services.CoverDownloadService" /> dedupes via the cover
///         state table keyed by the per-cover detail URL, so the cover image
///         is downloaded once but considered against each release in turn.
///     </para>
/// </summary>
public static partial class CoverArtTabParser
{
    [GeneratedRegex(@"/cover/group-(\d+)/cover-(\d+)/", RegexOptions.IgnoreCase)]
    private static partial Regex CoverIdRegex();

    public static List<ParsedCoverGroup> Parse(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        return Parse(doc);
    }

    public static List<ParsedCoverGroup> Parse(HtmlDocument doc)
    {
        var groups = new List<ParsedCoverGroup>();

        var h2s = doc.DocumentNode.SelectNodes("//h2");

        if(h2s is null) return groups;

        foreach(var h2 in h2s)
        {
            // The h2 wraps the platform name(s) in an <a> anchor and may have a
            // trailing <br>(suffix) such as "(Steam release)", so we use InnerText
            // (recursive) and collapse whitespace.
            string headingText = WebUtility.HtmlDecode(h2.InnerText);
            headingText = System.Text.RegularExpressions.Regex.Replace(headingText, @"\s+", " ").Trim();

            if(string.IsNullOrEmpty(headingText)) continue;

            // Skip page-level h2s that aren't cover-group separators. A genuine
            // cover-group h2 is followed by either a metadata <table> or a
            // <figure> before the next h2; if neither is present, ignore.
            (List<HtmlNode> figures, HtmlNode metaTable) = CollectGroupBlocks(h2);

            if(figures.Count == 0) continue;

            string packaging     = null;
            string videoStandard = null;
            var    countries     = new List<string>();

            if(metaTable is not null)
                ParseMetadataTable(metaTable, ref packaging, ref videoStandard, countries);

            var covers = ParseFigures(figures, out string detectedGroupId);

            if(covers.Count == 0) continue;

            // Split the h2 into individual platforms ("Foo and Bar and Baz") and
            // emit one group per platform. Parenthesised suffixes such as
            // "Windows(Steam release)" are preserved verbatim — the platform
            // matcher already normalises common name shapes.
            foreach(string platform in SplitPlatforms(headingText))
            {
                var group = new ParsedCoverGroup
                {
                    Platform      = platform,
                    Packaging     = packaging,
                    VideoStandard = videoStandard,
                    Countries     = new List<string>(countries),
                    Covers        = covers.Select(c => new ParsedCoverImage
                    {
                        Type          = c.Type,
                        CoverId       = c.CoverId,
                        DetailPageUrl = c.DetailPageUrl,
                        ThumbnailUrl  = c.ThumbnailUrl
                    }).ToList(),
                    GroupId = detectedGroupId
                };

                groups.Add(group);
            }
        }

        return groups;
    }

    static (List<HtmlNode> Figures, HtmlNode MetaTable) CollectGroupBlocks(HtmlNode h2)
    {
        var      figures   = new List<HtmlNode>();
        HtmlNode metaTable = null;

        var sib = h2.NextSibling;

        while(sib is not null)
        {
            if(sib.NodeType == HtmlNodeType.Element)
            {
                if(sib.Name == "h2") break;

                // First table encountered before any figure is the metadata table.
                if(sib.Name == "table" && metaTable is null && figures.Count == 0)
                    metaTable = sib;

                // <figure> is sometimes nested inside <div class="sensitive-blur-wrapper">.
                if(sib.Name == "figure")
                {
                    figures.Add(sib);
                }
                else
                {
                    var nested = sib.SelectNodes(".//figure");

                    if(nested is not null)
                        figures.AddRange(nested);
                }
            }

            sib = sib.NextSibling;
        }

        return (figures, metaTable);
    }

    static void ParseMetadataTable(HtmlNode table, ref string packaging, ref string videoStandard,
                                   List<string> countries)
    {
        var rows = table.SelectNodes(".//tr");

        if(rows is null) return;

        foreach(var row in rows)
        {
            var cells = row.SelectNodes("./td");

            if(cells is null || cells.Count < 2) continue;

            string label = WebUtility.HtmlDecode(cells[0].InnerText).Trim().TrimEnd(':').Trim();
            var    valueCell = cells[1];

            switch(label)
            {
                case "Packaging":
                    packaging = JoinCommaList(valueCell);

                    break;
                case "Video Standard":
                    videoStandard = JoinCommaList(valueCell);

                    break;
                case "Country":
                case "Countries":
                    foreach(string c in EnumerateCommaListWithoutFlags(valueCell))
                        if(!string.IsNullOrWhiteSpace(c) && !countries.Contains(c))
                            countries.Add(c);

                    break;
            }
        }
    }

    static string JoinCommaList(HtmlNode cell)
    {
        var items = cell.SelectNodes(".//ul[contains(@class,'commaList')]/li");

        if(items is null || items.Count == 0)
            return WebUtility.HtmlDecode(cell.InnerText).Trim();

        return string.Join(", ",
                           items.Select(li => WebUtility.HtmlDecode(li.InnerText).Trim())
                                .Where(s => !string.IsNullOrWhiteSpace(s)));
    }

    static IEnumerable<string> EnumerateCommaListWithoutFlags(HtmlNode cell)
    {
        var items = cell.SelectNodes(".//ul[contains(@class,'commaList')]/li");

        if(items is null) yield break;

        foreach(var li in items)
        {
            // Strip flag <img> children so "United States" doesn't carry "United States flag".
            string text = string.Concat(li.ChildNodes
                                           .Where(n => n.NodeType == HtmlNodeType.Text)
                                           .Select(n => WebUtility.HtmlDecode(n.InnerText)));

            text = text.Trim().TrimEnd(',').Trim();

            if(!string.IsNullOrWhiteSpace(text))
                yield return text;
        }
    }

    static List<ParsedCoverImage> ParseFigures(List<HtmlNode> figures, out string detectedGroupId)
    {
        var covers = new List<ParsedCoverImage>();
        detectedGroupId = null;

        foreach(var fig in figures)
        {
            var a = fig.SelectSingleNode(".//a[@href]");

            if(a is null) continue;

            string href = a.GetAttributeValue("href", "");
            Match  m    = CoverIdRegex().Match(href);

            if(!m.Success) continue;

            string groupId = m.Groups[1].Value;
            string coverId = m.Groups[2].Value;

            detectedGroupId ??= groupId;

            var img = fig.SelectSingleNode(".//img");
            string thumb = img?.GetAttributeValue("src", null);

            var    fc       = fig.SelectSingleNode(".//figcaption");
            string typeText = fc is null ? null : WebUtility.HtmlDecode(fc.InnerText).Trim();

            if(string.IsNullOrWhiteSpace(typeText))
                typeText = "Front Cover";

            covers.Add(new ParsedCoverImage
            {
                Type          = typeText,
                CoverId       = coverId,
                DetailPageUrl = href,
                ThumbnailUrl  = thumb
            });
        }

        return covers;
    }

    static IEnumerable<string> SplitPlatforms(string heading)
    {
        // "Foo and Bar and Baz" → ["Foo","Bar","Baz"]; preserve parenthesised suffixes.
        foreach(string part in heading.Split([" and "], System.StringSplitOptions.RemoveEmptyEntries))
        {
            string trimmed = part.Trim();
            if(!string.IsNullOrEmpty(trimmed)) yield return trimmed;
        }
    }
}
