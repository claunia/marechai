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
///     Parses the new MobyGames promo art page (post-2023 Vue redesign).
///     Each promo group lives in a <c>&lt;section id="promo-image-group-{id}"&gt;</c>
///     containing an <c>&lt;h2&gt;</c> with the group name and link, followed by
///     one or more <c>&lt;figure&gt;</c> elements inside a
///     <c>&lt;div class="img-holder mb"&gt;</c>.
/// </summary>
public static partial class PromoArtTabParser
{
    [GeneratedRegex(@"group-(\d+)", RegexOptions.Compiled)]
    private static partial Regex GroupIdRegex();

    [GeneratedRegex(@"image-(\d+)", RegexOptions.Compiled)]
    private static partial Regex ImageIdRegex();

    /// <summary>
    ///     Regex fallback: extracts image references directly from raw HTML,
    ///     bypassing HtmlAgilityPack entirely. Used when DOM parsing fails
    ///     (e.g. due to malformed Vue component attributes corrupting the tree).
    /// </summary>
    [GeneratedRegex(
        @"href=""[^""]*?/promo/group-(\d+)/image-(\d+)/""",
        RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex ImageHrefRegex();

    /// <summary>Extracts group name from any anchor linking to /promo/group-{id}/.</summary>
    [GeneratedRegex(
        @"<a[^>]+href=""[^""]*?/promo/group-(\d+)/""[^>]*>\s*([^<]+?)\s*</a>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex GroupLinkRegex();

    public static List<ParsedPromoArtGroup> Parse(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var groups = Parse(doc);

        // If XPath-based parsing found nothing, try regex fallback
        if(groups.Count == 0 && html.Contains("/promo/group-"))
            groups = ParseWithRegex(html);

        return groups;
    }

    public static List<ParsedPromoArtGroup> Parse(HtmlDocument doc)
    {
        var groups = new List<ParsedPromoArtGroup>();

        // Target the <section id="promo-image-group-{id}"> containers directly.
        // This bypasses the navbar/header Vue components whose massive JSON attributes
        // can corrupt HAP's DOM tree.
        var sections = doc.DocumentNode.SelectNodes("//section[starts-with(@id, 'promo-image-group-')]");

        if(sections is null) return groups;

        foreach(var section in sections)
        {
            // Extract group name and ID from the h2 anchor
            var groupLink = section.SelectSingleNode(".//h2//a[contains(@href, '/promo/group-')]");

            if(groupLink is null) continue;

            string groupName = WebUtility.HtmlDecode(groupLink.InnerText).Trim();
            string groupHref = groupLink.GetAttributeValue("href", "");
            var    groupMatch = GroupIdRegex().Match(groupHref);
            string groupId   = groupMatch.Success ? groupMatch.Groups[1].Value : null;

            // Also try extracting from the section id attribute
            if(groupId is null)
            {
                string sectionId = section.GetAttributeValue("id", "");

                // id="promo-image-group-90458" → extract 90458
                if(sectionId.StartsWith("promo-image-group-"))
                    groupId = sectionId["promo-image-group-".Length..];
            }

            var group = new ParsedPromoArtGroup
            {
                GroupName = groupName,
                GroupId   = groupId
            };

            // Find all <figure> elements within this section (may be nested in <div class="img-holder">)
            var figures = section.SelectNodes(".//figure");

            if(figures is not null)
            {
                foreach(var figure in figures)
                {
                    var a = figure.SelectSingleNode(".//a[@href]");

                    if(a is null) continue;

                    string href       = a.GetAttributeValue("href", "");
                    var    imageMatch  = ImageIdRegex().Match(href);

                    if(!imageMatch.Success) continue;

                    string imageId = imageMatch.Groups[1].Value;

                    // Skip duplicates
                    if(group.Images.Any(i => i.ImageId == imageId)) continue;

                    // Caption from <figcaption>
                    var    fc      = figure.SelectSingleNode(".//figcaption");
                    string caption = fc is null ? null : WebUtility.HtmlDecode(fc.InnerText).Trim();

                    if(string.IsNullOrWhiteSpace(caption)) caption = null;

                    // Thumbnail URL from <img> src
                    var    img          = figure.SelectSingleNode(".//img");
                    string thumbnailUrl = img?.GetAttributeValue("src", null);

                    group.Images.Add(new ParsedPromoArtImage
                    {
                        ImageId       = imageId,
                        DetailPageUrl = href,
                        Caption       = caption,
                        ThumbnailUrl  = thumbnailUrl
                    });
                }
            }

            if(group.Images.Count > 0)
                groups.Add(group);
        }

        return groups;
    }

    /// <summary>
    ///     Regex-only fallback that scans raw HTML for promo image href patterns.
    ///     Groups images by their group ID. Used when HAP DOM parsing fails.
    ///     Groups whose name cannot be determined are skipped entirely.
    /// </summary>
    static List<ParsedPromoArtGroup> ParseWithRegex(string html)
    {
        var groupMap = new Dictionary<string, ParsedPromoArtGroup>();

        // Extract group names from <a href="...promo/group-{id}/">Name</a> links
        var headingNames = new Dictionary<string, string>();

        foreach(Match hm in GroupLinkRegex().Matches(html))
        {
            string gid  = hm.Groups[1].Value;
            string name = WebUtility.HtmlDecode(hm.Groups[2].Value).Trim();

            if(!string.IsNullOrWhiteSpace(name))
                headingNames.TryAdd(gid, name);
        }

        // Extract all image references — only for groups whose name we found
        foreach(Match m in ImageHrefRegex().Matches(html))
        {
            string groupId = m.Groups[1].Value;
            string imageId = m.Groups[2].Value;

            // Skip groups where we couldn't determine the real name
            if(!headingNames.ContainsKey(groupId)) continue;

            if(!groupMap.TryGetValue(groupId, out var group))
            {
                group = new ParsedPromoArtGroup
                {
                    GroupId   = groupId,
                    GroupName = headingNames[groupId]
                };

                groupMap[groupId] = group;
            }

            if(group.Images.Any(i => i.ImageId == imageId)) continue;

            // Reconstruct the detail page URL from the matched href
            string fullHref = m.Value.Length > 6
                                  ? m.Value[6..^1] // strip href=" and trailing "
                                  : $"/promo/group-{groupId}/image-{imageId}/";

            group.Images.Add(new ParsedPromoArtImage
            {
                ImageId       = imageId,
                DetailPageUrl = fullHref
            });
        }

        return groupMap.Values.Where(g => g.Images.Count > 0).ToList();
    }
}
